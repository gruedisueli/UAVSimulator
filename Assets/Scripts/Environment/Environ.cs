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
    [Serializable]
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

        public Environ()
        {

        }
    }
}
