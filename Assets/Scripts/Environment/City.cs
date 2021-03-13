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
        private SerializableDictionary<string, DronePortBase> _dronePorts = new SerializableDictionary<string, DronePortBase>();
        public SerializableDictionary<string, DronePortBase> DronePorts
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
        private SerializableDictionary<string, ParkingStructureBase> _parkingStructures = new SerializableDictionary<string, ParkingStructureBase>();
        public SerializableDictionary<string, ParkingStructureBase> ParkingStructures
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
        private SerializableDictionary<string, RestrictionZoneBase> _restrictionZones = new SerializableDictionary<string, RestrictionZoneBase>();
        public SerializableDictionary<string, RestrictionZoneBase> RestrictionZones
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

        public City(CityOptions stats)
        {
            CityStats = stats;
        }
    }
}
