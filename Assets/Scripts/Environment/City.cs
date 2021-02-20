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
        public CityOptions _cityStats;
        public Vector3 _worldPos;
        public Vector3 _regionTileWorldCenter;
        public float _regionTileSideLength;
        public List<DronePort> _dronePorts;
        public List<ParkingStructure> _parkingStructures;
        public List<RestrictionZone> _restrictionZones;

        public City(Vector3 pos, Vector3 regionTileCenter, float tileSize, CityOptions stats)
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

            //NEED TO UPDATE/INTEGRATE

            //dP._position = pos;
            //dP._rotation = rot;
            //_dronePorts.Add(dP);

            return dP;
        }

    }
}
