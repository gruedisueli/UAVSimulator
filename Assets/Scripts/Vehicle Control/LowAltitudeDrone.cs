using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A type of drone that goes through a series of destinations not in the corridors themselves, within the altitude range of other low-altitude drones and not within corridor altitudes. @Eunu, correct?
/// </summary>
public class LowAltitudeDrone : DroneBase
{
    // Start is called before the first frame update
    private Queue<Vector3> operationPoints;//the list of points the drone will travel through. @Eunu correct?
    protected override void Move()
    {
        if (state == "move")
        {
            Vector3 rotation = targetPosition - transform.position;
            rotation.y = 0.0f;
            Quaternion wantedRotation = Quaternion.LookRotation(rotation, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime * vcs.speedMultiplier);

        if ( Vector3.Distance(transform.position, targetPosition) < approaching_threshold && wayPointsQueue.Count == 0 )
        {
            GetNextAction();
        }
    }

    /// <summary>
    /// Manages getting next action in state machine.
    /// </summary>
    protected override void GetNextAction()
    {
        if (state == "wait")
        {
            currentSpeed = 0;
            state = "idle";
        }
        else if (state == "takeoff")
        {
            wayPointsQueue = operationPoints;
            currentCommunicationPoint.SendMessage("FreeUp");
            currentSpeed = maxSpeed;
            state = "move";
        }
        else if (state == "move")
        {
            state = "pending";
            currentSpeed = 0;
            currentCommunicationPoint.SendMessage("RegisterInQueue", this.gameObject);
        }
        else if (state == "land")
        {
            if (destinationQueue.Count == 0)
            {
                state = "idle";
                TrafficControl tc = currentCommunicationPoint.GetComponent<TrafficControl>();
                tc.FreeUp();
                ParkEvent();
                isParked = true;
            }
            else
            {
                currentSpeed = 0;
                state = "wait";
                waitTimer = 0.0f;
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
