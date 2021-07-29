using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.DataStructure;
using UnityEngine;

namespace Assets.Scripts.SimulatorCore
{
    /// <summary>
    /// Provides analysis of the entire simulation. @Eunu, correct?
    /// </summary>
    public class SimulationAnalyzer : MonoBehaviour
    {
        //@Eunu please review my comments on all fields:
        public float averageSpeed; //{ get; set; }//average speed of all drones. @Eunu, correct?
        public float throughput; //{ get; set; }//@Eunu comment.
        public float grossEnergyConsumption; //{ get; set; }//total energy consumption since start of simulation. @Eunu correct? Or is it just current energy consumption right now?
        public float grossEmission; //{ get; set; }//total emissions since start of simulation. @Eunu correct? Or is it just current emissions right now?
        public List<GameObject> highNoiseBuildings;  //{ get; set; }//list of current buildings with "high noise"
        public List<GameObject> mediumNoiseBuildings; //{ get; set; }//list of current buildings with "medium noise"
        public List<GameObject> lowNoiseBuildings; //{ get; set; }//list of current buildings with "low noise"
        public List<GameObject> flyingDrones; //{ get; set; }//list of drones in the air right now.
        public List<Corridor> congestedCorridors; //{ get; set; }//list of corridors currently labeled as congested.
        public List<GameObject> congestedParkingStructures; //{ get; set; }//list of parking structures currently labeled as congested.
        public List<GameObject> congestedDronePorts; //{ get; set; }//list of drone ports currently labeled as congested.

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
        /// <summary>
        /// Add building to high noise building list.
        /// </summary>
        public void AddHighNoiseBuilding ( GameObject building )
        {
            if (!highNoiseBuildings.Contains(building)) highNoiseBuildings.Add(building);
        }
        /// <summary>
        /// Remove building from high noise building list.
        /// </summary>
        public void RemoveHighNoiseBuilding(GameObject building)
        {
            if (highNoiseBuildings.Contains(building)) highNoiseBuildings.Remove(building);
        }
        /// <summary>
        /// Add building to medium noise building list.
        /// </summary>
        public void AddMediumNoiseBuilding(GameObject building)
        {
            if (!mediumNoiseBuildings.Contains(building)) mediumNoiseBuildings.Add(building);
        }
        /// <summary>
        /// Remove building from medium noise building list.
        /// </summary>
        public void RemoveMediumNoiseBuilding(GameObject building)
        {
            if (mediumNoiseBuildings.Contains(building)) mediumNoiseBuildings.Remove(building);
        }
        /// <summary>
        /// Add building to low noise building list.
        /// </summary>
        public void AddLowNoiseBuilding(GameObject building)
        {
            if (!lowNoiseBuildings.Contains(building)) lowNoiseBuildings.Add(building);
        }
        /// <summary>
        /// Remove building from low noise building list.
        /// </summary>
        public void RemoveLowNoiseBuilding(GameObject building)
        {
            if (lowNoiseBuildings.Contains(building)) lowNoiseBuildings.Remove(building);
        }
        #endregion

        #region Drone Related Methods

        /// <summary>
        /// Add drone to flying drone list.
        /// </summary>
        public void AddFlyingDrone(GameObject drone)
        {
            throughput += drone.GetComponent<DroneBase>().capacity;
            flyingDrones.Add(drone);
        }

        /// <summary>
        /// Remove drone from flying drone list.
        /// </summary>
        public void RemoveFlyingDrone(GameObject drone)
        {
            throughput -= drone.GetComponent<DroneBase>().capacity;
            flyingDrones.Remove(drone);
        }


        #endregion

        #region Corridor Related Methods

        /// <summary>
        /// Add corridor to list of congested corridors.
        /// </summary>
        public void AddCongestedCorridors ( Corridor corridor )
        {
            congestedCorridors.Add(corridor);
        }

        /// <summary>
        /// Remove corridor from list of congested corridors.
        /// </summary>
        public void RemoveCongestedCorridors(Corridor corridor)
        {
            congestedCorridors.Remove(corridor);
        }
        #endregion

        #region Asset Related Methods

        /// <summary>
        /// Add parking structure to list of congested structures.
        /// </summary>
        public void AddCongestedParkingStructure(GameObject parkingStructure)
        {
            congestedParkingStructures.Add(parkingStructure);
        }

        /// <summary>
        /// Remove parking structure from list of congested structures.
        /// </summary>
        public void RemoveCongestedParkingStructure(GameObject parkingStructure)
        {
            congestedParkingStructures.Remove(parkingStructure);
        }

        /// <summary>
        /// Add drone port to list of congested structures.
        /// </summary>
        public void AddCongestedDronePort(GameObject dronePort)
        {
            congestedParkingStructures.Add(dronePort);
        }

        /// <summary>
        /// Remove drone port from list of congested structures.
        /// </summary>
        public void RemoveCongestedDronePort(GameObject dronePort)
        {
            congestedParkingStructures.Remove(dronePort);
        }

        #endregion

    }
}
