using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// This class is for controlling the current environemnt. 
    /// Tasks include things like loading and saving the environment from/to exterial files.
    /// The controller itself is not part of the save file. It's what talks to the save file and maintains the current state of the environment
    /// This class is thread safe, derived from singleton example on https://csharpindepth.com/articles/singleton
    /// </summary>
    public sealed class EnvironManager
    {
        private static EnvironManager _instance = null;
        private static readonly object _lock = new object();

        private EnvironManager()
        {
            CreateNew();
        }

        public static EnvironManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EnvironManager();
                    }
                    return _instance;
                }
            }
        }

        public Environ Environ { get; private set; } = new Environ();
        public string ActiveCity { get; private set; } = "";
        public Dictionary<string, DronePortAssetPack> DronePortAssets { get; private set; } = new Dictionary<string, DronePortAssetPack>();
        public Dictionary<string, ParkingStructureAssetPack> ParkingStructAssets { get; private set; } = new Dictionary<string, ParkingStructureAssetPack>();
        public Dictionary<string, RestrictionZoneAssetPack> RestrictionZoneAssets { get; private set; } = new Dictionary<string, RestrictionZoneAssetPack>();

        /// <summary>
        /// Adds city to current environment. False on failure.
        /// </summary>
        public bool AddCity(string guid, City city)
        {
            if (!Environ._cities.ContainsKey(guid))
            {
                Environ._cities.Add(guid, city);
                return true;
            }
            else
            {
                Debug.Log("Specified city already present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Removes city from current environment. False on failure.
        /// </summary>
        public bool RemoveCity(string guid)
        {
            if (Environ._cities.ContainsKey(guid))
            {
                Environ._cities.Remove(guid);
                return true;
            }
            else
            {
                Debug.Log("Specified city not present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Gets city at the specified guid, if present. Null if not.
        /// </summary>
        public City GetCity(string guid)
        {
            if (Environ._cities.ContainsKey(guid))
            {
                return Environ._cities[guid];
            }
            else
            {
                Debug.Log("Specified city not present in dictionary");
                return null;
            }
        }

        /// <summary>
        /// Gets current city dictionary.
        /// </summary>
        public SerializableDictionary<string, City> GetCities()
        {
            return Environ._cities;
        }

        /// <summary>
        /// Gets the city marked as "active". Null on failure.
        /// </summary>
        public City GetCurrentCity()
        {
            if (Environ._cities.ContainsKey(ActiveCity))
            {
                return Environ._cities[ActiveCity];
            }
            else
            {
                Debug.Log("Active city not present in dictionary");
                return null;
            }
        }


        /// <summary>
        /// Create an entirely new environment.
        /// </summary>
        public void CreateNew()
        {
            Environ = new Environ();
            ReadDronePorts();
            ReadParkingStructures();
        }

        /// <summary>
        /// Save current environment
        /// </summary>
        public void SaveFile()
        {
            using (StreamWriter writer = new StreamWriter(SerializationSettings.SAVE_PATH))
            {
                string json = JsonUtility.ToJson(Environ, true);
                writer.WriteLine(json);
                writer.Close();
                Debug.Log("File written to " + SerializationSettings.SAVE_PATH);
            }

        }

        /// <summary>
        /// Load an environment into the controller.
        /// </summary>
        public void LoadSaved()
        {
            using (StreamReader reader = new StreamReader(SerializationSettings.SAVE_PATH))
            {
                string json = reader.ReadToEnd();
                reader.Close();
                Environ = JsonUtility.FromJson<Environ>(json);
            }
        }

        /// <summary>
        /// Sets the currently active city for simulating
        /// </summary>
        public void SetActiveCity(string guid)
        {
            ActiveCity = guid;
        }

        /// <summary>
        /// Reads types of parking structures from assets.
        /// </summary>
        private void ReadParkingStructures()
        {
            ParkingStructAssets = new Dictionary<string, ParkingStructureAssetPack>();

            string sPath = SerializationSettings.ROOT + "\\Resources\\ParkingStructures";
            string rPath = "ParkingStructures/";

            //get DAT resources
            var files = Directory.GetFiles(sPath, "*.DAT");
            foreach (string filename in files)
            {
                // Read lines and parse DAT files
                StreamReader this_file = new StreamReader(filename);
                string type = filename.Substring(filename.LastIndexOf('\\') + 1, filename.Length - filename.LastIndexOf('\\') - 5);
                List<Vector3> spots = new List<Vector3>();
                string line;
                while ((line = this_file.ReadLine()) != null)
                {
                    line = line.Replace("(", "").Replace(")", "");
                    var splitted = line.Split(',');
                    Vector3 point = new Vector3(float.Parse(splitted[0]), float.Parse(splitted[1]), float.Parse(splitted[2]));
                    spots.Add(point);
                }

                var pfb = ReadPrefab(rPath, type);

                var pS = new ParkingStructure();
                pS.parkingSpots = spots;
                ParkingStructAssets.Add(type, new ParkingStructureAssetPack(pfb, pS));
            }

            //get JSON resources
            files = Directory.GetFiles(sPath, "*.JSON");
            foreach(string filename in files)
            {
                var pS = ReadJsonAsset<ParkingStructure>(filename);
                var pfb = ReadPrefab(rPath, pS.type);
                ParkingStructAssets.Add(pS.type, new ParkingStructureAssetPack(pfb, pS));
            }
        }

        /// <summary>
        /// Reads types of drone ports from assets.
        /// </summary>
        private void ReadDronePorts()
        {
            DronePortAssets = new Dictionary<string, DronePortAssetPack>();

            string sPath = SerializationSettings.ROOT + "\\Resources\\DronePorts";
            string rPath = "DronePorts/";
            var files = Directory.GetFiles(sPath, "*.JSON");
            foreach (var filename in files)
            {
                DronePort dp = ReadJsonAsset<DronePort>(filename);
                var pfb = ReadPrefab(rPath, dp.type);
                DronePortAssets.Add(dp.type, new DronePortAssetPack(pfb, dp));
            }
        }

        /// <summary>
        /// Reads types of restriction zones from assets
        /// </summary>
        private void ReadRestrictionZones()
        {
            RestrictionZoneAssets = new Dictionary<string, RestrictionZoneAssetPack>();

            string sPath = SerializationSettings.ROOT + "\\Resources\\RestrictionZones";
            string rPath = "RestrictionZones/";
            var files = Directory.GetFiles(sPath, "*.JSON");
            foreach(var filename in files)
            {
                var rS = ReadJsonAsset<RestrictionZone>(filename);
                var pfb = ReadPrefab(rPath, rS.type);
                RestrictionZoneAssets.Add(rS.type, new RestrictionZoneAssetPack(pfb, rS));
            }
        }

        /// <summary>
        /// Returns asset of specified type from json.
        /// </summary>
        private T ReadJsonAsset<T>(string fileName)
        {
            string json = File.ReadAllText(fileName, Encoding.UTF8);
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Reads prefab from resources of specified file name. Will load .prefab or .obj files. Null on failure.
        /// </summary>
        private GameObject ReadPrefab(string resourcePath, string name)
        {
            var pfb = Resources.Load<GameObject>(resourcePath + name);
            if (pfb == null)
            {
                Debug.Log("Cannot find prefab for " + name);
            }
            return pfb;
        }
    }
}
