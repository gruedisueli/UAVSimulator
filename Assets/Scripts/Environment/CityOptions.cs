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
    /// <summary>
    /// Class hold user-configurable city stats
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CityOptions
    {
        [JsonProperty]
        private string _name = "New City";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [JsonProperty]
        private SerVect3f _worldPos = new SerVect3f();
        public Vector3 WorldPos
        {
            get
            {
                return _worldPos.ToVector3();
            }
            set
            {
                _worldPos = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _regionTileWorldCenter = new SerVect3f();
        public Vector3 RegionTileWorldCenter
        {
            get
            {
                return _regionTileWorldCenter.ToVector3();
            }
            set
            {
                _regionTileWorldCenter = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private int _eastExt = 0;
        public int EastExt
        {
            get
            {
                return _eastExt;
            }
            set
            {
                _eastExt = value;
            }
        }

        [JsonProperty]
        private int _westExt = 0;
        public int WestExt
        {
            get
            {
                return _westExt;
            }
            set
            {
                _westExt = value;
            }
        }

        [JsonProperty]
        private int _northExt = 0;
        public int NorthExt
        {
            get
            {
                return _northExt;
            }
            set
            {
                _northExt = value;
            }
        }

        [JsonProperty]
        private int _southExt = 0;
        public int SouthExt
        {
            get
            {
                return _southExt;
            }
            set
            {
                _southExt = value;
            }
        }

        public CityOptions()
        {

        }

        public CityOptions(CityOptions cityOptions)
        {
            Name = cityOptions.Name;
            WorldPos = cityOptions.WorldPos;
            RegionTileWorldCenter = cityOptions.RegionTileWorldCenter;
            EastExt = cityOptions.EastExt;
            WestExt = cityOptions.WestExt;
            NorthExt = cityOptions.NorthExt;
            SouthExt = cityOptions.SouthExt;
        }
    }
}
