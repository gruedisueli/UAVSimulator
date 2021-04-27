using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.DataStructure;
using UnityEngine;

namespace Assets.Scripts.SimulatorCore
{
    public class SimulationAnalyzer : MonoBehaviour
    {
        public float averageSpeed { get; set; }
        public float throughput { get; set; }
        public float grossEnergyConsumption { get; set; }
        public float grossEmission { get; set; }
        public List<GameObject> highNoiseBuildings  { get; set; }
        public List<GameObject> mediumNoiseBuildings { get; set; }
        public List<GameObject> lowNoiseBuildings { get; set; }
        public List<GameObject> flyingDrones { get; set; }
        public List<Corridor> congestedCorridors { get; set; }
        public List<GameObject> congestedParkingStructures { get; set; }
        public List<GameObject> congestedDronePorts { get; set; }

        void Start()
        {
            averageSpeed = 0.0f;
            throughput = 0.0f;
            grossEmission = 0.0f;
            grossEnergyConsumption = 0.0f;
            highNoiseBuildings = new List<GameObject>();
            mediumNoiseBuildings = new List<GameObject>();
            lowNoiseBuildings = new List<GameObject>();
            flyingDrones = new List<GameObject>();
            congestedCorridors = new List<Corridor>();
            congestedParkingStructures = new List<GameObject>();
            congestedDronePorts = new List<GameObject>();
        }

        #region Building Related Methods
        public void AddHighNoiseBuilding ( GameObject building )
        {
            if (!highNoiseBuildings.Contains(building)) highNoiseBuildings.Add(building);
        }
        public void RemoveHighNoiseBuilding(GameObject building)
        {
            if (highNoiseBuildings.Contains(building)) highNoiseBuildings.Remove(building);
        }
        public void AddMediumNoiseBuilding(GameObject building)
        {
            if (!mediumNoiseBuildings.Contains(building)) mediumNoiseBuildings.Add(building);
        }
        public void RemoveMediumNoiseBuilding(GameObject building)
        {
            if (mediumNoiseBuildings.Contains(building)) mediumNoiseBuildings.Remove(building);
        }
        public void AddLowNoiseBuilding(GameObject building)
        {
            if (!lowNoiseBuildings.Contains(building)) lowNoiseBuildings.Add(building);
        }
        public void RemoveLowNoiseBuilding(GameObject building)
        {
            if (lowNoiseBuildings.Contains(building)) lowNoiseBuildings.Remove(building);
        }
        #endregion

        #region Drone Related Methods
        public void AddFlyingDrone(GameObject drone)
        {
            throughput += drone.GetComponent<DroneBase>().capacity;
            flyingDrones.Add(drone);
        }
        public void RemoveFlyingDrone(GameObject drone)
        {
            throughput -= drone.GetComponent<DroneBase>().capacity;
            flyingDrones.Remove(drone);
        }


        #endregion

        #region Corridor Related Methods
        public void AddCongestedCorridors ( Corridor corridor )
        {
            congestedCorridors.Add(corridor);
        }
        public void RemoveCongestedCorridors(Corridor corridor)
        {
            congestedCorridors.Remove(corridor);
        }
        #endregion

        #region Asset Related Methods
        public void AddCongestedParkingStructure(GameObject parkingStructure)
        {
            congestedParkingStructures.Add(parkingStructure);
        }
        public void RemoveCongestedParkingStructure(GameObject parkingStructure)
        {
            congestedParkingStructures.Remove(parkingStructure);
        }

        public void AddCongestedDronePort(GameObject dronePort)
        {
            congestedParkingStructures.Add(dronePort);
        }
        public void RemoveCongestedDronePort(GameObject dronePort)
        {
            congestedParkingStructures.Remove(dronePort);
        }

        #endregion

    }
}
