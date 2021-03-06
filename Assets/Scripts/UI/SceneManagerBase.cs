using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Assets.Scripts.Environment;
using Assets.Scripts.Serialization;

using UnityEngine;

namespace Assets.Scripts.UI
{
    public abstract class SceneManagerBase : MonoBehaviour
    {
        public Material _restrictionZoneMaterial;
        public Dictionary<string, SceneDronePort> _dronePorts;
        public Dictionary<string, SceneParkingStructure> _parkingStructures;

        public List<GameObject> _destinationCollections;
        public Dictionary<GameObject, List<GameObject>> _routes;
        protected Dictionary<string, GameObject> _restrictionZones;

        private void Start()
        {
            _dronePorts = new Dictionary<string, SceneDronePort>();
            _parkingStructures = new Dictionary<string, SceneParkingStructure>();
            _destinationCollections = new List<GameObject>();
            _routes = new Dictionary<GameObject, List<GameObject>>();
            _restrictionZones = new Dictionary<string, GameObject>();

            Init();

            CreateRoutes();
        }

        /// <summary>
        /// All derived classes should call this instead of "Start()", for use in view-specific initialization
        /// </summary>
        protected abstract void Init();

        public void CreateRoutes()
        {

            _destinationCollections = new List<GameObject>();
            foreach (var kvp in _dronePorts)
            {
                _destinationCollections.Add(kvp.Value.SceneGameObject);
            }


            // create straight paths first (elevation == 0 means straight paths)
            // paths whose elevation is closest to the assigned elevation will be examined first
            // if there is an obstacle in the middle, construct a walkaround path and register with the new elevation

            for (int i = 0; i < _destinationCollections.Count; i++)
            {
                for (int j = 0; j < _destinationCollections.Count; j++)
                {
                    if (i != j)
                    {
                        GameObject origin = _destinationCollections[i];
                        GameObject destination = _destinationCollections[j];
                        if (Vector3.Distance(origin.transform.position, destination.transform.position) < EnvironSettings.RANGE_LIMIT)
                        {
                            if (!_routes.ContainsKey(origin)) _routes.Add(origin, new List<GameObject>());

                            List<GameObject> this_origin_adjacent_nodes = _routes[origin];
                            this_origin_adjacent_nodes.Add(destination);
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Gets number of parking spots in current scene.
        /// </summary>
        public int GetParkingCapacity()
        {
            int parking_capacity = 0;
            foreach (var kvp in _parkingStructures)
            {
                parking_capacity += kvp.Value.ParkingStructureSpecs.ParkingSpots.Count;
            }
            return parking_capacity;
        }


        
    }
}
