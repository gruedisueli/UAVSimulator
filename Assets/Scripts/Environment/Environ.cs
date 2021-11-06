using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

using Assets.Scripts.Serialization;

using Mapbox.Utils;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// This serializable class contains EVERYTHING that you would want to need to save/load a configuration.
    /// It is serializable for this reason.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Environ
    {

        /// <summary>
        /// Center of simulation, and mapbox origin.
        /// </summary>
        [JsonProperty]
        private SerVect2d _centerLatLong = new SerVect2d(37.7648, -122.463); //san francisco default
        public Vector2d CenterLatLong
        {
            get
            {
                return _centerLatLong.ToVector2d();
            }
            set
            {
                _centerLatLong = new SerVect2d(value);
            }
        }

        /// <summary>
        /// Dictionary of all cities, keyed by guid.
        /// </summary>
        [JsonProperty]
        private  Dictionary<string, City> _cities = new Dictionary<string, City>();
        public Dictionary<string, City> Cities
        {
            get
            {
                return _cities;
            }
            set
            {
                _cities = value;
            }
        }

        [JsonProperty]
        private Dictionary<string, AirspaceTile> _airspaceTiles = new Dictionary<string, AirspaceTile>();
        /// <summary>
        /// The airspace tiles for this region.
        /// </summary>
        public Dictionary<string, AirspaceTile> AirspaceTiles
        {
            get
            {
                return _airspaceTiles;
            }
            set
            {
                _airspaceTiles = value;
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

        public Environ()
        {

        }
    }
}
