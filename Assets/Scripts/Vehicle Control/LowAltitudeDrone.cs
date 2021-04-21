using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowAltitudeDrone : DroneBase
{
    // Start is called before the first frame update
    private Queue<Vector3> operationPoints;
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

    protected override void GetNextAction()
    {
        if (state == "wait")
        {
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
                state = "wait";
                waitTimer = 0.0f;
            }
        }
    }

    public void SetOperationPoints ( Queue<Vector3> operationPts )
    {
        operationPoints = operationPts;
    }
}
