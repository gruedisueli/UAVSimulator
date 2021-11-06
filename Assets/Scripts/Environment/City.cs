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
    [JsonObject(MemberSerialization.OptIn)]
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

        public City(CityOptions stats)
        {
            CityStats = stats;
        }

        /// <summary>
        /// Empty constructor for json deserialization
        /// </summary>
        public City()
        {

        }
    }
}
