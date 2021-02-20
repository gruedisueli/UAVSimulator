using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

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

        //A NOTE ON THESE LISTS: Dictionaries are not serializable by the Unity to json converter.  Here's a solution.
        //and fields have to be public to be serialized by json converter because "SerializeField" attribute appears not to work.
        public List<string> _cityGuids = new List<string>();
        public List<City> _cities = new List<City>();

        public Environ()
        {

        }

        /// <summary>
        /// Returns all the cities.
        /// </summary>
        public List<City> GetCities()
        {
            return _cities;
        }

        /// <summary>
        /// Returns guids for all the cities.
        /// </summary>
        public List<string> GetCityGuids()
        {
            return _cityGuids;
        }

        /// <summary>
        /// Gets city of specified key from list, if it exists. Null if not.
        /// </summary>
        public City GetCity(string guid)
        {
            if (_cityGuids.Contains(guid))
            {
                int i = _cityGuids.IndexOf(guid);
                if (i < _cities.Count)
                {
                    return _cities[i];
                }
                else
                {
                    Debug.Log("City list does not contain specified index");
                    return null;
                }
            }
            else
            {
                Debug.Log("Specified city not in list");
                return null;
            }
        }

        /// <summary>
        /// Adds new city to dictionary. True on sucess
        /// </summary>
        public bool AddCity(string guid, City city)
        {
            if (!_cityGuids.Contains(guid))
            {
                _cityGuids.Add(guid);
                _cities.Add(city);
                return true;
            }
            else
            {
                Debug.Log("Specified guid is already present in city database");
                return false;
            }
        }

        /// <summary>
        /// Removes specified city from lists, if it exists.
        /// </summary>
        public void RemoveCity(string guid)
        {
            if (_cityGuids.Contains(guid))
            {
                int i = _cityGuids.IndexOf(guid);
                _cityGuids.RemoveAt(i);
                if (i < _cities.Count)
                {
                    _cities.RemoveAt(i);
                }
                else
                {
                    Debug.Log("City list does not contain specified index");
                    return;
                }
            }
            else
            {
                Debug.Log("Specified city not in list");
                return;
            }
        }
    }
}
