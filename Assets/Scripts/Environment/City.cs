using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class City
    {
        [JsonProperty]
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

        [JsonProperty]
        private Dictionary<string, DronePortBase> _dronePorts = new Dictionary<string, DronePortBase>();
        public Dictionary<string, DronePortBase> DronePorts
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

        [JsonProperty]
        private Dictionary<string, ParkingStructureBase> _parkingStructures = new Dictionary<string, ParkingStructureBase>();
        public Dictionary<string, ParkingStructureBase> ParkingStructures
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

        [JsonProperty]
        private Dictionary<string, RestrictionZoneBase> _restrictionZones = new Dictionary<string, RestrictionZoneBase>();
        public Dictionary<string, RestrictionZoneBase> RestrictionZones
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
