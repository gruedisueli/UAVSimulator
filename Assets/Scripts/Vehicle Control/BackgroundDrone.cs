using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundDrone : DroneBase
{
    protected override void Move()
    {
        currentSpeed = maxSpeed * vcs.speedMultiplier;
        if (Vector3.Distance(targetPosition, transform.position) < arrival_threshold)
        {
            if ( wayPointsQueue.Count == 0 ) wayPointsQueue = new Queue<Vector3>(vcs.sceneManager.FindPath(transform.position, vcs.GetRandomPointXZ(transform.position.y), 5));
            targetPosition = wayPointsQueue.Dequeue();
        }
        else
        {
            Quaternion wantedRotation = Quaternion.LookRotation(targetPosition - transform.position, transform.up);
            if (state == "move") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
        }
    }
}
