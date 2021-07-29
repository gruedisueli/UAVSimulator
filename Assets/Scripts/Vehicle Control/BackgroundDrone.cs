using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A drone that exists for the purpose of adding visual activity to simulation. Has no functional purpose beyond visual effect. Is not part of simulation collision detection. @Eunu comment.
/// Background drones just fly from random point to point. They do not interact with structures or other drones. They do not respond to transport calls from source to destination.
/// </summary>
public class BackgroundDrone : DroneBase
{
    /// <summary>
    /// Move operation specific to this type of drone.
    /// </summary>
    protected override void Move()
    {
        //@Eunu comment on what's happening in here.

        currentSpeed = maxSpeed * vcs.speedMultiplier;
        if (Vector3.Distance(targetPosition, transform.position) < arrival_threshold)
        {
            if ( wayPointsQueue.Count == 0 ) wayPointsQueue = new Queue<Vector3>(vcs.sceneManager.FindPath(transform.position, vcs.GetRandomPointXZ(transform.position.y), 5, 1 << 8 | 1 << 9 | 1 << 13)); //note use of bit-wise shift operators as a handy way to create a layer mask
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
