using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Class hold user-configurable city stats
    /// </summary>
    [Serializable]
    public class CityOptions
    {
        [SerializeField]
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

        [SerializeField]
        private Vector3 _worldPos = new Vector3();
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
        private Vector3 _regionTileWorldCenter = new Vector3();
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

        [SerializeField]
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

        [SerializeField]
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

        [SerializeField]
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

        [SerializeField]
        private int _population = 0;
        public int Population
        {
            get
            {
                return _population;
            }
            set
            {
                _population = value;
            }
        }

        [SerializeField]
        private int _jobs = 0;
        public int Jobs
        {
            get
            {
                return _jobs;
            }
            set
            {
                _jobs = value;
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
            Population = cityOptions.Population;
            Jobs = cityOptions.Jobs;
        }
    }
}
