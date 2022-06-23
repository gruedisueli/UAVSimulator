using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using UnityEngine;

namespace Assets.Scripts.DataStructure
{
    /// <summary>
    /// A pathway for a corridor drone.
    /// </summary>
    [Serializable]
    public class Corridor
    {

        public delegate void OnCongestionLevelChangeDelegate(Corridor corridor, int congestionLevel);
        public event OnCongestionLevelChangeDelegate OnCongestionLevelChange;

        public GameObject origin;
        public GameObject destination;
        public float elevation;
        public Queue<Vector3> wayPoints;
        public List<GameObject> dronesInCorridor;
        public float speedSum;
        public float maxSpeed;

        private int _congestionLevel;

        /// <summary>
        /// Get or set the congestion level for the corridor. On set to new value, invokes event.
        /// </summary>
        public int congestionLevel
        {
            get
            {
                return _congestionLevel;
            }
            set
            {
                if (_congestionLevel == value) return;
                else
                {
                    _congestionLevel = value;
                    if (OnCongestionLevelChange != null)
                    {
                        OnCongestionLevelChange(this, _congestionLevel);
                    }
                }
            }
        }
        public float averageSpeed
        {
            get
            {
                return speedSum / dronesInCorridor.Count;
            }
        }

        /// <summary>
        /// Compares two corridors for positional equivalency
        /// </summary>
        public bool Equals(Corridor p1, Corridor p2)
        {
            if (p1.origin.transform.position.Equals(p2.origin.transform.position) && p1.destination.transform.position.Equals(p2.destination.transform.position) && Mathf.Abs(p1.elevation - p2.elevation) < 0.001) return true;
            else return false;
        }

        /// <summary>
        /// Builds corridor between two points at specified elevation.
        /// </summary>
        public Corridor(GameObject org, GameObject dest, float elev, float maxSpd)
        {
            origin = org;
            destination = dest;
            elevation = elev;
            dronesInCorridor = new List<GameObject>();
            speedSum = 0.0f;
            _congestionLevel = 0;
            maxSpeed = maxSpd;
        }

        public void AddDrone ( GameObject drone )
        {
            CorridorDrone cd = drone.GetComponent<CorridorDrone>();
            cd.OnInCorridorSpeedChange += SpeedChangeHandler;
            dronesInCorridor.Add(drone);
        }

        public void RemoveDrone(GameObject drone)
        {
            CorridorDrone cd = drone.GetComponent<CorridorDrone>();
            cd.OnInCorridorSpeedChange -= SpeedChangeHandler;
            dronesInCorridor.Remove(drone);
        }

        private void SpeedChangeHandler(float oldSpeed, float newSpeed)
        {
            speedSum -= oldSpeed;
            speedSum += newSpeed;
            if (averageSpeed / maxSpeed > 0.7f && averageSpeed / maxSpeed < 0.8f) congestionLevel = 1;
            else if (averageSpeed / maxSpeed > 0.6f && averageSpeed / maxSpeed < 0.7f) congestionLevel = 2;
            else if (averageSpeed / maxSpeed <= 0.6f && dronesInCorridor.Count > 0) congestionLevel = 3;
            else congestionLevel = 0;
        }


    }

    /// <summary>
    /// The network of all the corridors in the simulation. @Eunu comment.
    /// </summary>
    [Serializable]
    public class Network
    {
        public List<GameObject> vertices { get; set; }
        public List<Corridor> corridors { get; set; }
        public Dictionary<GameObject, List<Corridor>> outEdges { get; set; }
        public Dictionary<GameObject, List<Corridor>> inEdges { get; set; }


        public Network()
        {
            vertices = new List<GameObject>();
            corridors = new List<Corridor>();
            outEdges = new Dictionary<GameObject, List<Corridor>>();
            inEdges = new Dictionary<GameObject, List<Corridor>>();
        }
    }


}
