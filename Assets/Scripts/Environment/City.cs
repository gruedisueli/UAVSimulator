using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class City
    {
        [SerializeField]
        private CityOptions _cityStats;
        public CityOptions CityStats
        {
            get
            {
                return _cityStats;
            }
            set
            {
                _cityStats = value;
            }
        }

        [SerializeField]
        private Vector3 _worldPos;
        public Vector3 WorldPos
        {
            get
            {
                return _worldPos;
            }
            set
            {
                _worldPos = value;
            }
        }

        [SerializeField]
        private Vector3 _regionTileWorldCenter;
        public Vector3 RegionTileWorldCenter
        {
            get
            {
                return _regionTileWorldCenter;
            }
            set
            {
                _regionTileWorldCenter = value;
            }
        }

        [SerializeField]
        private float _regionTileSideLength;
        public float RegionTileSideLength
        {
            get
            {
                return _regionTileSideLength;
            }
            set
            {
                _regionTileSideLength = value;
            }
        }

        [SerializeField]
        private SerializableDictionary<string, DronePort> _dronePorts = new SerializableDictionary<string, DronePort>();
        public SerializableDictionary<string, DronePort> DronePorts
        {
            get
            {
                return _dronePorts;
            }
            set
            {
                _dronePorts = value;
            }
        }

        [SerializeField]
        private SerializableDictionary<string, ParkingStructure> _parkingStructures = new SerializableDictionary<string, ParkingStructure>();
        public SerializableDictionary<string, ParkingStructure> ParkingStructures
        {
            get
            {
                return _parkingStructures;
            }
            set
            {
                _parkingStructures = value;
            }
        }

        [SerializeField]
        private SerializableDictionary<string, RestrictionZone> _restrictionZones = new SerializableDictionary<string, RestrictionZone>();
        public SerializableDictionary<string, RestrictionZone> RestrictionZones
        {
            get
            {
                return _restrictionZones;
            }
            set
            {
                _restrictionZones = value;
            }
        }

        public City(Vector3 pos, Vector3 regionTileCenter, float tileSize, CityOptions stats)
        {
            CityStats = stats;
            WorldPos = pos;
            RegionTileWorldCenter = regionTileCenter;
            RegionTileSideLength = tileSize;
        }
    }
}
