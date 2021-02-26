using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class City
    {
        public CityOptions _cityStats;
        public Vector3 _worldPos;
        public Vector3 _regionTileWorldCenter;
        public float _regionTileSideLength;
        public SerializableDictionary<string, DronePort> _dronePorts = new SerializableDictionary<string, DronePort>();
        public SerializableDictionary<string, ParkingStructure> _parkingStructures = new SerializableDictionary<string, ParkingStructure>();
        public SerializableDictionary<string, RestrictionZone> _restrictionZones = new SerializableDictionary<string, RestrictionZone>();

        public City(Vector3 pos, Vector3 regionTileCenter, float tileSize, CityOptions stats)
        {
            _cityStats = stats;
            _worldPos = pos;
            _regionTileWorldCenter = regionTileCenter;
            _regionTileSideLength = tileSize;
        }
    }
}
