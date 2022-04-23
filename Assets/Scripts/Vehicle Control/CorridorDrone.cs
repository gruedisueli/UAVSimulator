using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataStructure;
using Assets.Scripts.Environment;

/// <summary>
/// A type of drone confined to traveling in corridors
/// </summary>
public class CorridorDrone : DroneBase
{
    public delegate void OnInCorridorSpeedChangeDelegate(float oldSpeed, float newSpeed);
    public event OnInCorridorSpeedChangeDelegate OnInCorridorSpeedChange;

    private Corridor _currentCorridor;
    private float _inCorridorSpeed = 0.0f;
    public float inCorridorSpeed
    {
        get
        {
            return _inCorridorSpeed;
        }
        private set
        {
            if (_inCorridorSpeed == value) return;
            else
            {
                if ( OnInCorridorSpeedChange != null )
                {
                    OnInCorridorSpeedChange(_inCorridorSpeed, value);
                }
                _inCorridorSpeed = value;
            }
        }
    }

    protected override Queue<Vector3> GetWayPointsToNextDestination()
    {
        foreach (Corridor c in _vcs.DroneNetwork.outEdges[CurrentCommunicationPoint])
        {
            if (c.destination.Equals(DestinationQueue.Peek()))
            {
                _currentCorridor = c;
                c.AddDrone(this.gameObject); //register drone to corridor.
                return (new Queue<Vector3>(c.wayPoints));
            }
        }
        return new Queue<Vector3>();
    }

    /// <summary>
    /// Move function specific to this type of drone.
    /// </summary>
    protected override void Move()
    {
        // Move() can be different between:
        // (1) Corridor drones (w/ strategic deconfliction, w/o tactical deconfliction)
        //     : Corridor drones only live in corridors so it only needs to check the separation to front
        //     : Restriction zone avoidance is already taken into account by strategic deconfliction
        if (State == "move" && Physics.Raycast(gameObject.transform.position + Vector3.Normalize(TargetPosition - gameObject.transform.position) * (_vcs.DRONE_SCALE * 2), TargetPosition - gameObject.transform.position, 25.00f, _droneLayermask))
        {
            inCorridorSpeed = 0.0f;
            return;
        }
        else
        {
            if (State == "move")
            {
                Vector3 rotation = TargetPosition - transform.position;
                rotation.y = 0.0f;
                Quaternion wantedRotation = Quaternion.LookRotation(rotation, transform.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * YawSpeed);
                inCorridorSpeed = CurrentSpeed;
            }
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, CurrentSpeed * Time.deltaTime * EnvironManager.Instance.Environ.SimSettings.SimulationSpeedMultiplier);
            
        }
    }

    /// <summary>
    /// Set state to pending, register drone in queue
    /// </summary>
    protected override void Pending()
    {
        State = "pending";
        CurrentSpeed = 0;
        CurrentCommunicationPoint.SendMessage("RegisterInQueue", this.gameObject);
        inCorridorSpeed = 0.0f;
    }

    /// <summary>
    /// Set state to landing and deregister drone from corridor.
    /// </summary>
    public override void LandGranted()
    {
        State = "land";
        inCorridorSpeed = 0.0f;
        CurrentSpeed = LandingSpeed;
        _currentCorridor.RemoveDrone(this.gameObject);
    }

}
