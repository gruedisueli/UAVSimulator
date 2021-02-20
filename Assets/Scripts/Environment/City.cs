using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class City
    {
        //public CityStats CityStats
        //{
        //    get
        //    {
        //        return _cityStats;
        //    }
        //    set
        //    {
        //        _cityStats = value;
        //    }
        //}
        //public Vector3 WorldPos
        //{
        //    get
        //    {
        //        return _worldPos != null ? new Vector3(_worldPos[0], _worldPos[1], _worldPos[2]) : new Vector3();
        //    }
        //    set
        //    {
        //        _worldPos = new float[] { value.x, value.y, value.z };
        //    }
        //}
        //public Vector3 RegionTileWorldCenter
        //{
        //    get
        //    {
        //        return _regionTileWorldCenter != null ? new Vector3(_regionTileWorldCenter[0], _regionTileWorldCenter[1], _regionTileWorldCenter[2]) : new Vector3();
        //    }
        //    set
        //    {
        //        _regionTileWorldCenter = new float[] { value.x, value.y, value.z };
        //    }
        //}
        //public float RegionTileSideLength
        //{
        //    get
        //    {
        //        return _regionTileSideLength;
        //    }
        //    set
        //    {
        //        _regionTileSideLength = value;
        //    }
        //}
        //public List<DronePort> DronePorts
        //{
        //    get
        //    {
        //        return _dronePorts;
        //    }
        //    set
        //    {
        //        _dronePorts = value;
        //    }
        //}

        //unity serialization is incredibly dumb and requires fields to be public
        //their "SerializeField" attribute does not work.
        public CityStats _cityStats;
        public Vector3 _worldPos;
        public Vector3 _regionTileWorldCenter;
        public float _regionTileSideLength;
        public List<DronePort> _dronePorts;

        public City(Vector3 pos, Vector3 regionTileCenter, float tileSize, CityStats stats)
        {
            _cityStats = stats;
            _worldPos = pos;
            _regionTileWorldCenter = regionTileCenter;
            _regionTileSideLength = tileSize;
            _dronePorts = new List<DronePort>();
        }

        public DronePort AddDronePort(Vector3 pos, double rot)
        {
            DronePort dP = new DronePort();
            dP._position = pos;
            dP._rotation = rot;
            _dronePorts.Add(dP);

            return dP;
        }

    }
}
