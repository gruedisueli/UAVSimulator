using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

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
        public Vector2d centerLatLong = new Vector2d(37.7648, -122.463); //san francisco default

        /// <summary>
        /// Dictionary of all cities, keyed by guid.
        /// </summary>
        public SerializableDictionary<string, City> _cities = new SerializableDictionary<string, City>();

        public Environ()
        {

        }
    }
}
