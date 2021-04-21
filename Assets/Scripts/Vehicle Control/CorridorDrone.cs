using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataStructure;

public class CorridorDrone : DroneBase
{
    Corridor currentCorridor { get; set; }

    protected override Queue<Vector3> GetWayPointsToNextDestination()
    {
        foreach (Corridor c in vcs.sceneManager.network.outEdges[currentCommunicationPoint])
        {
            if (c.destination.Equals(destinationQueue.Peek()))
            {
                currentCorridor = c;
                return (new Queue<Vector3>(c.wayPoints));
            }
        }
        return new Queue<Vector3>();
    }
    protected override void Move()
    {
        // Move() can be different between:
        // (1) Corridor drones (w/ strategic deconfliction, w/o tactical deconfliction)
        //     : Corridor drones only live in corridors so it only needs to check the separation to front
        //     : Restriction zone avoidance is already taken into account by strategic deconfliction
        if (state == "move" && Physics.Raycast(gameObject.transform.position + Vector3.Normalize(targetPosition - gameObject.transform.position) * (vcs.DRONE_SCALE * 2), targetPosition - gameObject.transform.position, 25.00f, DRONE_LAYERMASK))
        {
            return;
        }
        else
        {
            if (state == "move")
            {
                Vector3 rotation = targetPosition - transform.position;
                rotation.y = 0.0f;
                Quaternion wantedRotation = Quaternion.LookRotation(rotation, transform.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * yawSpeed);
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime * vcs.speedMultiplier);
        }
    }
}
