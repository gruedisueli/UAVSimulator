using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.DataStructure;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts.SimulatorCore
{
    /// <summary>
    /// Provides analysis of the entire simulation.
    /// </summary>
    public class SimulationAnalyzer : MonoBehaviour
    {
        public float averageSpeed; //{ get; set; }
        public float throughput; //{ get; set; }
        //public float grossEnergyConsumption; //{ get; set; }
        //public float grossEmission; //{ get; set; }
        //public List<GameObject> highNoiseBuildings;  //{ get; set; }//list of current buildings with "high noise"
        //public List<GameObject> mediumNoiseBuildings; //{ get; set; }//list of current buildings with "medium noise"
        //public List<GameObject> lowNoiseBuildings; //{ get; set; }//list of current buildings with "low noise"
        public List<DroneBase> flyingDrones; //{ get; set; }//list of drones in the air right now.
        public List<Corridor> congestedCorridors; //{ get; set; }//list of corridors currently labeled as congested.
        public List<GameObject> congestedParkingStructures; //{ get; set; }//list of parking structures currently labeled as congested.
        public List<GameObject> congestedDronePorts; //{ get; set; }//list of drone ports currently labeled as congested.

        private float _statsUpdateInterval = 1;//seconds
        private float _lastStatsUpdate = 0;
        private bool _isPlaying = false;


        private float _noiseUpdateInterval = 0.05f;//how frequently to check the noise, in seconds
        private float _lastNoiseUpdate = 0.0f;//last unity time we checked noise

        void Start()
        {
            averageSpeed = 0.0f;
            throughput = 0.0f;
            //grossEmission = 0.0f;
            //grossEnergyConsumption = 0.0f;
            //highNoiseBuildings = new List<GameObject>();
            //mediumNoiseBuildings = new List<GameObject>();
            //lowNoiseBuildings = new List<GameObject>();
            flyingDrones = new List<DroneBase>();
            congestedCorridors = new List<Corridor>();
            congestedParkingStructures = new List<GameObject>();
            congestedDronePorts = new List<GameObject>();
            UpdateGlobalNoiseValues();
        }

        void Update()
        {
            var t = Time.unscaledTime;
            if (_isPlaying && t - _lastStatsUpdate > _statsUpdateInterval)
            {
                _lastStatsUpdate = t;
                float sum = 0;
                foreach (var d in flyingDrones)
                {
                    sum += d.CurrentSpeed;
                }

                averageSpeed = sum / flyingDrones.Count;

                throughput = 0;
                foreach (var d in flyingDrones)
                {
                    throughput += d.DroneSettingsReference.Capacity;
                }
            }

            if (_isPlaying && t - _lastNoiseUpdate > _noiseUpdateInterval)
            {
                _lastNoiseUpdate = t;
                UpdateGlobalNoiseValues();
            }

        }

        private void UpdateGlobalNoiseValues()
        {
            //var p = Shader.GetGlobalVectorArray("_dronePositions");
            //var n = Shader.GetGlobalFloatArray("_noiseRadii");
            //https://forum.unity.com/threads/shader-read-static-variable-in-c-script.171914/
            var positions = new List<Vector4>();
            var radii = new List<float>();
            for (int i = 0; i < 1000; i++)
            {
                if (i >= flyingDrones.Count)
                {
                    positions.Add(new Vector4(999999, 999999, 999999, 999999));
                    radii.Add(0);
                    continue;
                }
                positions.Add(flyingDrones[i].transform.position);
                radii.Add(flyingDrones[i].NoiseRadius);
            }
            Shader.SetGlobalVectorArray("_dronePositions", positions);
            Shader.SetGlobalFloatArray("_noiseRadii", radii);
        }
        /// <summary>
        /// Performs reset actions when stopping, otherwise commences analysis
        /// </summary>
        public void PlayStop(bool isPlaying)
        {
            _isPlaying = isPlaying;
            if (!isPlaying)
            {
                flyingDrones.Clear();
                congestedCorridors.Clear();
                congestedParkingStructures.Clear();
                congestedDronePorts.Clear();
                //highNoiseBuildings.Clear();
                //mediumNoiseBuildings.Clear();
                //lowNoiseBuildings.Clear();
                averageSpeed = 0;
                throughput = 0;
            }
        }

        #region Building Related Methods
        ///// <summary>
        ///// Add building to high noise building list.
        ///// </summary>
        //public void AddHighNoiseBuilding ( GameObject building )
        //{
        //    if (!highNoiseBuildings.Contains(building)) highNoiseBuildings.Add(building);
        //}
        ///// <summary>
        ///// Remove building from high noise building list.
        ///// </summary>
        //public void RemoveHighNoiseBuilding(GameObject building)
        //{
        //    if (highNoiseBuildings.Contains(building)) highNoiseBuildings.Remove(building);
        //}
        ///// <summary>
        ///// Add building to medium noise building list.
        ///// </summary>
        //public void AddMediumNoiseBuilding(GameObject building)
        //{
        //    if (!mediumNoiseBuildings.Contains(building)) mediumNoiseBuildings.Add(building);
        //}
        ///// <summary>
        ///// Remove building from medium noise building list.
        ///// </summary>
        //public void RemoveMediumNoiseBuilding(GameObject building)
        //{
        //    if (mediumNoiseBuildings.Contains(building)) mediumNoiseBuildings.Remove(building);
        //}
        ///// <summary>
        ///// Add building to low noise building list.
        ///// </summary>
        //public void AddLowNoiseBuilding(GameObject building)
        //{
        //    if (!lowNoiseBuildings.Contains(building)) lowNoiseBuildings.Add(building);
        //}
        ///// <summary>
        ///// Remove building from low noise building list.
        ///// </summary>
        //public void RemoveLowNoiseBuilding(GameObject building)
        //{
        //    if (lowNoiseBuildings.Contains(building)) lowNoiseBuildings.Remove(building);
        //}
        #endregion

        #region Drone Related Methods

        /// <summary>
        /// Add drone to flying drone list.
        /// </summary>
        public void AddFlyingDrone(DroneBase drone)
        {
            if (!flyingDrones.Contains(drone))
            {
                flyingDrones.Add(drone);
            }
        }

        /// <summary>
        /// Remove drone from flying drone list.
        /// </summary>
        public void RemoveFlyingDrone(DroneBase drone)
        {
            if (flyingDrones.Contains(drone))
            {
                flyingDrones.Remove(drone);
            }
        }


        #endregion

        #region Corridor Related Methods

        ///// <summary>
        ///// Add corridor to list of congested corridors.
        ///// </summary>
        //public void AddCongestedCorridors ( Corridor corridor )
        //{
        //    if (!congestedCorridors.Contains(corridor))
        //    {
        //        congestedCorridors.Add(corridor);
        //    }
        //}

        ///// <summary>
        ///// Remove corridor from list of congested corridors.
        ///// </summary>
        //public void RemoveCongestedCorridors(Corridor corridor)
        //{
        //    if (congestedCorridors.Contains(corridor))
        //    {
        //        congestedCorridors.Remove(corridor);
        //    }
        //}
        #endregion

        #region Asset Related Methods

        ///// <summary>
        ///// Add parking structure to list of congested structures.
        ///// </summary>
        //public void AddCongestedParkingStructure(GameObject parkingStructure)
        //{
        //    if (!congestedParkingStructures.Contains(parkingStructure))
        //    {
        //        congestedParkingStructures.Add(parkingStructure);
        //    }
        //}

        ///// <summary>
        ///// Remove parking structure from list of congested structures.
        ///// </summary>
        //public void RemoveCongestedParkingStructure(GameObject parkingStructure)
        //{
        //    if (congestedParkingStructures.Contains(parkingStructure))
        //    {
        //        congestedParkingStructures.Remove(parkingStructure);
        //    }
        //}

        ///// <summary>
        ///// Add drone port to list of congested structures.
        ///// </summary>
        //public void AddCongestedDronePort(GameObject dronePort)
        //{
        //    if (!congestedDronePorts.Contains(dronePort))
        //    {
        //        congestedDronePorts.Add(dronePort);
        //    }
        //}

        ///// <summary>
        ///// Remove drone port from list of congested structures.
        ///// </summary>
        //public void RemoveCongestedDronePort(GameObject dronePort)
        //{
        //    if (congestedDronePorts.Contains(dronePort))
        //    {
        //        congestedDronePorts.Remove(dronePort);
        //    }
        //}

        #endregion

    }
}
