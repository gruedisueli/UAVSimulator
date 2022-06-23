using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

/// <summary>
/// A drone that exists for the purpose of adding visual activity to simulation. Has no functional purpose beyond visual effect. Is not part of simulation collision detection. 
/// Background drones just fly from random point to point. They do not interact with structures or other drones. They do not respond to transport calls from source to destination.
/// </summary>
public class BackgroundDrone : DroneBase
{
    /// <summary>
    /// Move operation specific to this type of drone.
    /// </summary>
    protected override void Move()
    {
        CurrentSpeed = DroneSettingsReference.MaxSpeed_MPS * EnvironManager.Instance.Environ.SimSettings.SimulationSpeedMultiplier;
        if (Vector3.Distance(TargetPosition, transform.position) < ArrivalThreshold)
        {
            if ( WayPointsQueue.Count == 0 ) WayPointsQueue = new Queue<Vector3>(_vcs.FindPath(transform.position, _vcs.GetRandomPointXZ(transform.position.y), 5, 1 << 8 | 1 << 9 | 1 << 13 | 1 << 14)); //note use of bit-wise shift operators as a handy way to create a layer mask
            TargetPosition = WayPointsQueue.Dequeue();
        }
        else
        {
            Quaternion wantedRotation = Quaternion.LookRotation(TargetPosition - transform.position, transform.up);
            if (State == "move") transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * DroneSettingsReference.YawSpeed);
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, CurrentSpeed * Time.deltaTime);
        }
    }
}
