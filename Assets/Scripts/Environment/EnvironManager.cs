using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

using Newtonsoft.Json;

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
        public Material SelectedSceneElementMat { get; private set; } = null;
        public Material DefaultSceneElementMat { get; private set; } = null;
        public Material RestrictionZoneMaterial { get; private set; } = null;
        public GameObject CityInfoPanelPrefab { get; private set; } = null;
        public GameObject DronePortInfoPanelPrefab { get; private set; } = null;
        public GameObject ParkingInfoPanelPrefab { get; private set; } = null;
        public GameObject RestrictionInfoPanelPrefab { get; private set; } = null;
        
        //public Dictionary<string, RestrictionZoneAssetPack> RestrictionZoneAssets { get; private set; } = new Dictionary<string, RestrictionZoneAssetPack>();

        /// <summary>
        /// Adds new drone port to environment. False on failure.
        /// </summary>
        public bool AddDronePort(string guid, DronePortBase dP)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return false;
            }
            if (!city.DronePorts.ContainsKey(guid))
            {
                city.DronePorts.Add(guid, dP);
                return true;
            }
            else
            {
                Debug.LogError("Specified drone port already present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Adds new parking structure to environment. False on failure.
        /// </summary>
        public bool AddParkingStructure(string guid, ParkingStructureBase pS)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return false;
            }
            if (!city.ParkingStructures.ContainsKey(guid))
            {
                city.ParkingStructures.Add(guid, pS);
                return true;
            }
            else
            {
                Debug.LogError("Specified parking structure already present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Adds new restriction zone to environment. False on failure.
        /// </summary>
        public bool AddRestrictionZone(string guid, RestrictionZoneBase rZ)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return false;
            }
            if (!city.RestrictionZones.ContainsKey(guid))
            {
                city.RestrictionZones.Add(guid, rZ);
                return true;
            }
            else
            {
                Debug.LogError("Specified restriction zone already present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Removes drone port from environment. False on failure.
        /// </summary>
        public bool RemoveDronePort(string guid)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return false;
            }
            if (city.DronePorts.ContainsKey(guid))
            {
                city.DronePorts.Remove(guid);
                return true;
            }
            else
            {
                Debug.LogError("Specified drone port not present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Removes parking structure from environment. False on failure.
        /// </summary>
        public bool RemoveParkingStructure(string guid)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return false;
            }
            if (city.ParkingStructures.ContainsKey(guid))
            {
                city.ParkingStructures.Remove(guid);
                return true;
            }
            else
            {
                Debug.LogError("Specified parking structure not present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Removes restriction zone from environment. False on failure.
        /// </summary>
        public bool RemoveRestrictionZone(string guid)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return false;
            }
            if (city.RestrictionZones.ContainsKey(guid))
            {
                city.RestrictionZones.Remove(guid);
                return true;
            }
            else
            {
                Debug.LogError("Specified restriction zone not present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Gets drone port. Null on failure.
        /// </summary>
        public DronePortBase GetDronePort(string guid)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return null;
            }
            if (city.DronePorts.ContainsKey(guid))
            {
                return city.DronePorts[guid];
            }
            else
            {
                Debug.LogError("Specified drone port not present in dictionary");
                return null;
            }
        }

        ///// <summary>
        ///// Sets drone port. False on failure.
        ///// </summary>
        //public bool SetDronePort(string guid, DronePortBase dP)
        //{
        //    var city = GetCurrentCity();
        //    if (city == null)
        //    {
        //        return false;
        //    }
        //    if (city.DronePorts.ContainsKey(guid))
        //    {
        //        city.DronePorts[guid] = dP;
        //        return true;
        //    }
        //    else
        //    {
        //        Debug.LogError("Specified drone port not present in dictionary");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Gets parking structure. Null on failure.
        /// </summary>
        public ParkingStructureBase GetParkingStructure(string guid)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return null;
            }
            if (city.ParkingStructures.ContainsKey(guid))
            {
                return city.ParkingStructures[guid];
            }
            else
            {
                Debug.LogError("Specified parking structure not present in dictionary");
                return null;
            }
        }

        ///// <summary>
        ///// Sets parking structure. False on failure.
        ///// </summary>
        //public bool SetParkingStructure(string guid, ParkingStructureBase pS)
        //{
        //    var city = GetCurrentCity();
        //    if (city == null)
        //    {
        //        return false;
        //    }
        //    if (city.ParkingStructures.ContainsKey(guid))
        //    {
        //        city.ParkingStructures[guid] = pS;
        //        return true;
        //    }
        //    else
        //    {
        //        Debug.LogError("Specified parking structure not present in dictionary");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Gets restriction zone. Null on failure.
        /// </summary>
        public RestrictionZoneBase GetRestrictionZone(string guid)
        {
            var city = GetCurrentCity();
            if (city == null)
            {
                return null;
            }
            if (city.RestrictionZones.ContainsKey(guid))
            {
                return city.RestrictionZones[guid];
            }
            else
            {
                Debug.LogError("Specified restriction zone not present in dictionary");
                return null;
            }
        }

        ///// <summary>
        ///// Sets restriction zone. False on failure.
        ///// </summary>
        //public bool SetRestrictionZone(string guid, RestrictionZoneBase rZ)
        //{
        //    var city = GetCurrentCity();
        //    if (city == null)
        //    {
        //        return false;
        //    }
        //    if (city.RestrictionZones.ContainsKey(guid))
        //    {
        //        city.RestrictionZones[guid] = rZ;
        //        return true;
        //    }
        //    else
        //    {
        //        Debug.LogError("Specified restriction zone not present in dictionary");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Adds city to current environment. False on failure.
        /// </summary>
        public bool AddCity(string guid, City city)
        {
            if (!Environ.Cities.ContainsKey(guid))
            {
                Environ.Cities.Add(guid, city);
                return true;
            }
            else
            {
                Debug.LogError("Specified city already present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Removes city from current environment. False on failure.
        /// </summary>
        public bool RemoveCity(string guid)
        {
            if (Environ.Cities.ContainsKey(guid))
            {
                Environ.Cities.Remove(guid);
                return true;
            }
            else
            {
                Debug.LogError("Specified city not present in dictionary");
                return false;
            }
        }

        /// <summary>
        /// Gets city at the specified guid, if present. Null if not.
        /// </summary>
        public City GetCity(string guid)
        {
            if (Environ.Cities.ContainsKey(guid))
            {
                return Environ.Cities[guid];
            }
            else
            {
                Debug.LogError("Specified city not present in dictionary");
                return null;
            }
        }

        /// <summary>
        /// Gets current city dictionary.
        /// </summary>
        public Dictionary<string, City> GetCities()
        {
            return Environ.Cities;
        }

        /// <summary>
        /// Gets the city marked as "active". Null on failure.
        /// </summary>
        public City GetCurrentCity()
        {
            if (Environ.Cities.ContainsKey(ActiveCity))
            {
                return Environ.Cities[ActiveCity];
            }
            else
            {
                Debug.LogError("Active city not present in dictionary");
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
            ReadMaterialAssets();
            ReadInfoPanelPrefabs();
        }

        /// <summary>
        /// Save current environment
        /// </summary>
        public void SaveFile()
        {
            SerializeJsonFile(Environ, SerializationSettings.SAVE_PATH);
            Debug.Log("File written to " + SerializationSettings.SAVE_PATH);
        }

        /// <summary>
        /// Load an environment into the controller.
        /// </summary>
        public void LoadSaved()
        {
            Environ = DeserializeJsonFile<Environ>(SerializationSettings.SAVE_PATH);
            Debug.Log("File read from " + SerializationSettings.SAVE_PATH);
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

                var pfb = AssetUtils.ReadPrefab(rPath, type);

                var pS = new ParkingStructureCustom();
                pS.ParkingSpots = spots;
                pS.Type = type;
                ParkingStructAssets.Add(type, new ParkingStructureAssetPack(pfb, pS));
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
                DronePortCustom dp = DeserializeJsonFile<DronePortCustom>(filename);
                var pfb = AssetUtils.ReadPrefab(rPath, dp.Type);
                DronePortAssets.Add(dp.Type, new DronePortAssetPack(pfb, dp));
            }
        }

        /// <summary>
        /// Gets resources for prefabs of info panels
        /// </summary>
        private void ReadInfoPanelPrefabs()
        {
            string rPath = "GUI/";
            CityInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "CityInfoPanel");
            DronePortInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "DronePortInfoPanel");
            ParkingInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "ParkingInfoPanel");
            RestrictionInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "RestrictionInfoPanel");
        }

        private T DeserializeJsonFile<T>(string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All; //required for keeping track of derived classes.
            using (StreamReader sR = new StreamReader(path))
            {
                using (JsonReader reader = new JsonTextReader(sR))
                {
                    return serializer.Deserialize<T>(reader);
                }
            }
        }

        private void SerializeJsonFile(object obj, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented; //makes output file easier to read
            serializer.TypeNameHandling = TypeNameHandling.All; //required for keeping track of derived classes.

            using (StreamWriter sW = new StreamWriter(path))
            {
                using (JsonWriter writer = new JsonTextWriter(sW))
                {
                    serializer.Serialize(writer, obj);
                }
            }
        }

        /// <summary>
        /// Get relevant materials from resources folder
        /// </summary>
        private void ReadMaterialAssets()
        {
            string rPath = "Materials/";
            DefaultSceneElementMat = ReadMaterial(rPath, "DefaultElement");
            SelectedSceneElementMat = ReadMaterial(rPath, "SelectedElement");
            RestrictionZoneMaterial = ReadMaterial(rPath, "RestrictionZone");
        }

        /// <summary>
        /// Reads material from resources of specified file name. Null on failure.
        /// </summary>
        private Material ReadMaterial(string resourcePath, string name)
        {
            var mat = Resources.Load<Material>(resourcePath + name);
            if (mat == null)
            {
                Debug.LogError("Cannot find material for " + name);
            }
            return mat;
        }
    }
}
