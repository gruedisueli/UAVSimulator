using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

/// <summary>
/// A type of drone that goes through a series of destinations not in the corridors themselves
/// </summary>
public class LowAltitudeDrone : DroneBase
{
    private Queue<Vector3> operationPoints;
    protected override void Move()
    {
        if (State == "move")
        {
            Vector3 rotation = TargetPosition - transform.position;
            rotation.y = 0.0f;
            Quaternion wantedRotation = Quaternion.LookRotation(rotation, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * YawSpeed);
        }
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, CurrentSpeed * Time.deltaTime * EnvironManager.Instance.Environ.SimSettings.SimulationSpeedMultiplier);

        if ( Vector3.Distance(transform.position, TargetPosition) < ArrivalThreshold && WayPointsQueue.Count == 0 )
        {
            GetNextAction();
        }
    }

    /// <summary>
    /// Manages getting next action in state machine.
    /// </summary>
    protected override void GetNextAction()
    {
        if (State == "wait")
        {
            CurrentSpeed = 0;
            State = "idle";
        }
        else if (State == "takeoff")
        {
            WayPointsQueue = operationPoints;
            CurrentCommunicationPoint.SendMessage("FreeUp");
            CurrentSpeed = MaxSpeed;
            State = "move";
        }
        else if (State == "move")
        {
            State = "pending";
            CurrentSpeed = LandingSpeed;
            CurrentCommunicationPoint.SendMessage("RegisterInQueue", this.gameObject);
        }
        else if (State == "land")
        {
            if (DestinationQueue.Count == 0)
            {
                State = "idle";
                TrafficControl tc = CurrentCommunicationPoint.GetComponent<TrafficControl>();
                tc.FreeUp();
                ParkEvent();
                IsParked = true;
            }
            else
            {
                CurrentSpeed = 0;
                State = "wait";
                WaitTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// Sets operation points for this drone.
    /// </summary>
    public void SetOperationPoints ( Queue<Vector3> operationPts )
    {
        operationPoints = operationPts;
    }
}
