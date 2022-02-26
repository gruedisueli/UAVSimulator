using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using UnityEngine;

using Mapbox.VectorTile;
using Mapbox.Unity.MeshGeneration.Data;

using Newtonsoft.Json;

using Assets.Scripts.Serialization;
using Assets.Scripts.MapboxCustom;

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
        public string OpenedFile { get; private set; } = "";
        public string ActiveCity { get; private set; } = "";
        /// <summary>
        /// Last position of the camera, X, Z, and S, where "S" is Camera component "Size" parameter
        /// </summary>
        public float[] LastCamXZS { get; set; } = null;
        public Dictionary<string, DronePortAssetPack> DronePortAssets { get; private set; } = new Dictionary<string, DronePortAssetPack>();
        public Dictionary<string, ParkingStructureAssetPack> ParkingStructAssets { get; private set; } = new Dictionary<string, ParkingStructureAssetPack>();
        public Dictionary<string, RestrictionZoneAssetPack> RestrictionZoneAssets { get; private set; } = new Dictionary<string, RestrictionZoneAssetPack>();
        public Material SelectedSceneElementMat { get; private set; } = null;
        public Material DefaultSceneElementMat { get; private set; } = null;
        public Material RestrictionZoneMaterial { get; private set; } = null;
        public GameObject CityInfoPanelPrefab { get; private set; } = null;
        public GameObject DronePortInfoPanelPrefab { get; private set; } = null;
        public GameObject ParkingRectInfoPanelPrefab { get; private set; } = null;
        public GameObject ParkingCustomInfoPanelPrefab { get; private set; } = null;
        public GameObject RestrictionInfoPanelRectPrefab { get; private set; } = null;
        public GameObject RestrictionInfoPanelCylPrefab { get; private set; } = null;
        public GameObject RestrictionInfoPanelCylStackedPrefab { get; private set; } = null;
        public GameObject AddButtonPrefab { get; private set; } = null;
        public GameObject DroneIconPrefab { get; private set; } = null;
        public FOA_MapboxSettings MapboxSettings { get; private set; } = null;
        public GameObject ProgressBarPrefab { get; private set; } = null;
        public GameObject ParkingSpritePrefab { get; private set; } = null;
        public GameObject PortSpritePrefab { get; private set; } = null;
        public VehicleControlSystem VCS { get; set; } = null;


        /// <summary>
        /// Gets airspace data from our tileset on Mapbox
        /// </summary>
        public void DownloadAirspace(UnityTile t)
        {

            var id = t.CanonicalTileId;
            Debug.Log($"Starting airspace download for tile {id}");
            string prefix = "https://api.mapbox.com/v4/grudy.0odj258s/";
            string suffix = ".mvt?access_token=sk.eyJ1IjoiZ3J1ZHkiLCJhIjoiY2t2OG12bjRqMDNmZTJwdDJqNmc5eHZvNCJ9.oFQO2C4TCm2v8Qxxtrw86g";

            string url = prefix + id.Z.ToString() + "/" + id.X.ToString() + "/" + id.Y.ToString() + suffix;
            byte[] b = null;

            b = TryDownload(url);

            if (b == null)
            {
                Debug.Log($"Download from {url} failed");
                return;
            }
            Debug.Log($"Downloaded {url}");

            var reader = new VectorTileReader(b);
            var layer = reader.GetLayer("Class_Airspace-apq25l");

            var aTile = new AirspaceTile(layer, t);

            Instance.Environ.AirspaceTiles.Add(id.ToString(), aTile);
        }

        /// <summary>
        /// Tries to download from url. Null on failure.
        /// </summary>
        private byte[] TryDownload(string url)
        {
            byte[] b = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream str = response.GetResponseStream())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            str.CopyTo(memoryStream);
                            b = memoryStream.ToArray();
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            return b;
        }

        /// <summary>
        /// Adds new drone port to environment. False on failure.
        /// </summary>
        public bool AddDronePort(string guid, DronePortBase dP)
        {
            if (Environ == null)
            {
                return false;
            }
            if (!Environ.DronePorts.ContainsKey(guid))
            {
                Environ.DronePorts.Add(guid, dP);
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
            if (Environ == null)
            {
                return false;
            }
            if (!Environ.ParkingStructures.ContainsKey(guid))
            {
                Environ.ParkingStructures.Add(guid, pS);
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
            if (Environ == null)
            {
                return false;
            }
            if (!Environ.RestrictionZones.ContainsKey(guid))
            {
                Environ.RestrictionZones.Add(guid, rZ);
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
            if (Environ == null)
            {
                return false;
            }
            if (Environ.DronePorts.ContainsKey(guid))
            {
                Environ.DronePorts.Remove(guid);
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
            if (Environ == null)
            {
                return false;
            }
            if (Environ.ParkingStructures.ContainsKey(guid))
            {
                Environ.ParkingStructures.Remove(guid);
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
            if (Environ == null)
            {
                return false;
            }
            if (Environ.RestrictionZones.ContainsKey(guid))
            {
                Environ.RestrictionZones.Remove(guid);
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
            if (Environ == null)
            {
                return null;
            }
            if (Environ.DronePorts.ContainsKey(guid))
            {
                return Environ.DronePorts[guid];
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
            if (Environ == null)
            {
                return null;
            }
            if (Environ.ParkingStructures.ContainsKey(guid))
            {
                return Environ.ParkingStructures[guid];
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
            if (Environ == null)
            {
                return null;
            }
            if (Environ.RestrictionZones.ContainsKey(guid))
            {
                return Environ.RestrictionZones[guid];
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
            ReadRestrictionZones();
            ReadMaterialAssets();
            ReadInfoPanelPrefabs();
            ReadButtonPrefabs();
            ReadIconPrefabs();
            ReadMapboxSettings();
            ReadProgressBarPrefabs();
            if (!Directory.Exists(SerializationSettings.SAVE_PATH))
            {
                Directory.CreateDirectory(SerializationSettings.SAVE_PATH);
            }
        }

        /// <summary>
        /// Returns list of all saved files in directory. Does not include path or suffix in names
        /// </summary>
        public string[] GetAllSaveFiles()
        {
            var names = Directory.GetFiles(SerializationSettings.SAVE_PATH, "*.json");
            if (names != null)
            {
                for(int i = 0; i < names.Length; i++)
                {
                    var fragments = names[i].Split('/');
                    if (fragments != null)
                    {
                        var lastFrag = fragments[fragments.Length - 1];
                        names[i] = lastFrag.Substring(0, lastFrag.Length - 5);//remove suffix
                    }
                }
            }
            return names;
        }

        /// <summary>
        /// Returns true if file exists.
        /// </summary>
        public bool DoesFileExist(string name)
        {
            return File.Exists(SerializationSettings.SAVE_PATH + name + ".json");
        }

        /// <summary>
        /// Checks filename for validity.
        /// </summary>
        public bool IsFilenameValid(string name)
        {
            return name != "" && name.All(char.IsLetterOrDigit);
        }

        /// <summary>
        /// Save current environment
        /// </summary>
        public void SaveFile(string name)
        {
            string path = SerializationSettings.SAVE_PATH + name + ".json";
            SerializeJsonFile(Environ, path);
            Debug.Log("File written to " + path);
        }

        /// <summary>
        /// Load an environment into the controller.
        /// </summary>
        public void LoadSaved(string name)
        {
            string path = SerializationSettings.SAVE_PATH + name + ".json";
            if (!File.Exists(path)) return;
            Environ = DeserializeJsonFile<Environ>(path);
            OpenedFile = name;
            Debug.Log("File read from " + path);
        }

        /// <summary>
        /// Renames a file. True on success
        /// </summary>
        public bool RenameFile(string oldName, string newName)
        {
            if (IsFilenameValid(oldName) && IsFilenameValid(newName) && DoesFileExist(oldName) && !DoesFileExist(newName))
            {
                string oldPath = SerializationSettings.SAVE_PATH + oldName + ".json";
                string newPath = SerializationSettings.SAVE_PATH + newName + ".json";
                File.Move(oldPath, newPath);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Deletes a file. True on success
        /// </summary>
        public bool DeleteFile(string name)
        {
            if (IsFilenameValid(name) && DoesFileExist(name))
            {
                string path = SerializationSettings.SAVE_PATH + name + ".json";
                File.Delete(path);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the currently active city for simulating
        /// </summary>
        public void SetActiveCity(string guid)
        {
            ActiveCity = guid;
        }

        /// <summary>
        /// Updates access token in MapboxSettings file. Writes to actual file.
        /// </summary>
        public void SetAccessToken(string token)
        {
            MapboxSettings.AccessToken = token;
            SerializeJsonFile(MapboxSettings, SerializationSettings.ROOT + "\\Resources\\Mapbox\\MapboxConfiguration.txt");
        }

        /// <summary>
        /// Reads types of parking structures from assets.
        /// </summary>
        private void ReadParkingStructures()
        {
            ParkingStructAssets = new Dictionary<string, ParkingStructureAssetPack>();

            string rPath = "ParkingStructures/";
            var textAssets = Resources.LoadAll<TextAsset>(rPath);
            if (textAssets == null)
            {
                Debug.LogError("Could not locate parking structure text assets");
                return;
            }
            //get DAT and prefab resources
            foreach (var t in textAssets)
            {
                var lines = t.text.Split("\r\n".ToCharArray());
                // Read lines and parse CSV files
                List<Vector3> spots = new List<Vector3>();
                foreach(var line in lines)
                {
                    var cleaned = line.Replace("(", "").Replace(")", "");//legacy from .DAT file...consider exporting .CSV files from source instead of .DAT...we changed extension of .DAT assets to .CSV
                    var splitted = cleaned.Split(',');
                    if (splitted.Length != 3) continue;
                    Vector3 point = new Vector3(float.Parse(splitted[0]), float.Parse(splitted[1]), float.Parse(splitted[2]));
                    spots.Add(point);
                }

                var pfb = AssetUtils.ReadPrefab(rPath, t.name);

                var pS = new ParkingStructureCustom();
                pS.ParkingSpots = spots;
                pS.Type = t.name;
                ParkingStructAssets.Add(t.name, new ParkingStructureAssetPack(pfb, pS, null, ParkingStructCategory.Custom));
            }

            //get PNG resources (some types don't have CSV or prefabs)
            var textures = Resources.LoadAll<Sprite>(rPath);
            foreach (var t in textures)
            {
                var type = t.name;
                if (ParkingStructAssets.ContainsKey(type))
                {
                    ParkingStructAssets[type].PreviewImage = t;
                }
                else
                {
                    ParkingStructAssets.Add(type, new ParkingStructureAssetPack(null, null, t, ParkingStructCategory.Rect));
                }
            }

            ParkingSpritePrefab = Resources.Load<GameObject>("GUI/ParkingIcon");
        }

        /// <summary>
        /// Reads types of drone ports from assets.
        /// </summary>
        private void ReadDronePorts()
        {
            DronePortAssets = new Dictionary<string, DronePortAssetPack>();
            string rPath = "DronePorts/";
            var textAssets = Resources.LoadAll<TextAsset>(rPath);
            if (textAssets == null)
            {
                Debug.LogError("Could not locate drone port text assets");
                return;
            }
            foreach (var j in textAssets)
            {
                DronePortCustom dp = DeserializeJsonString<DronePortCustom>(j.text);
                var pfb = AssetUtils.ReadPrefab(rPath, dp.Type);
                DronePortAssets.Add(dp.Type, new DronePortAssetPack(pfb, dp, null, DronePortCategory.Custom));
            }
            
            //get PNG resources (some types don't have DAT or prefabs)
            var textures = Resources.LoadAll<Sprite>(rPath);
            foreach (var t in textures)
            {
                var type = t.name;
                if (DronePortAssets.ContainsKey(type))
                {
                    DronePortAssets[type].PreviewImage = t;
                }

                #region disabled rectangular drone ports here:

                //else
                //{
                //    DronePortAssets.Add(type, new DronePortAssetPack(null, null, sprite, DronePortCategory.Rect));
                //}


                #endregion
            }

            PortSpritePrefab = Resources.Load<GameObject>("GUI/PortIcon");
        }

        /// <summary>
        /// Reads types of restriction zones from assets.
        /// </summary>
        private void ReadRestrictionZones()
        {
            RestrictionZoneAssets = new Dictionary<string, RestrictionZoneAssetPack>();
            string rPath = "RestrictionZones/";
            var textures = Resources.LoadAll<Sprite>(rPath);
            foreach (var t in textures)
            {
                var type = t.name;
                if (RestrictionZoneAssets.ContainsKey(type))
                {
                    RestrictionZoneAssets[type].PreviewImage = t;
                }
                else
                {
                    RestrictionZoneCategory c;
                    if (type.Contains("Rect"))
                    {
                        c = RestrictionZoneCategory.Rect;
                    }
                    else if (type.Contains("Cyl_Stacked"))
                    {
                        c = RestrictionZoneCategory.CylindricalStacked;
                    }
                    else
                    {
                        c = RestrictionZoneCategory.Cylindrical;
                    }
                    RestrictionZoneAssets.Add(type, new RestrictionZoneAssetPack(null, null, t, c));
                }
            }
        }

        /// <summary>
        /// Reads Mapbox settings from assets.
        /// </summary>
        private void ReadMapboxSettings()
        {
            string rPath = "Mapbox/MapboxConfiguration";
            var settings = Resources.Load<TextAsset>(rPath);
            if (settings == null)
            {
                Debug.LogError("Could not load Mapbox settings file");
                return;
            }
            MapboxSettings = DeserializeJsonString<FOA_MapboxSettings>(settings.text);
        }

        /// <summary>
        /// Gets resources for prefabs of info panels
        /// </summary>
        private void ReadInfoPanelPrefabs()
        {
            string rPath = "GUI/";
            CityInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "CityInfoPanel");
            DronePortInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "DronePortInfoPanel");
            ParkingRectInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "ParkingRectInfoPanel");
            ParkingCustomInfoPanelPrefab = AssetUtils.ReadPrefab(rPath, "ParkingCustomInfoPanel");
            RestrictionInfoPanelRectPrefab = AssetUtils.ReadPrefab(rPath, "RestrictionInfoPanelRect");
            RestrictionInfoPanelCylPrefab = AssetUtils.ReadPrefab(rPath, "RestrictionInfoPanelCyl");
            RestrictionInfoPanelCylStackedPrefab = AssetUtils.ReadPrefab(rPath, "RestrictionInfoPanelCylStacked");
        }

        /// <summary>
        /// Gets resources for prefabs of progress bars
        /// </summary>
        private void ReadProgressBarPrefabs()
        {
            string rPath = "GUI/";
            ProgressBarPrefab = AssetUtils.ReadPrefab(rPath, "ProgressBar");
        }

        /// <summary>
        /// Gets resources for prefabs of icons.
        /// </summary>
        private void ReadIconPrefabs()
        {
            string rPath = "GUI/";
            DroneIconPrefab = AssetUtils.ReadPrefab(rPath, "DroneIcon");
        }

        /// <summary>
        /// Gets resources for prefabs of buttons
        /// </summary>
        private void ReadButtonPrefabs()
        {
            string rPath = "GUI/";
            AddButtonPrefab = AssetUtils.ReadPrefab(rPath, "Button_Add");
        }


        /// <summary>
        /// Creates object of specified type from json. Returns null on failure.
        /// </summary>
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

        /// <summary>
        /// Creates object of specified type from json string. https://stackoverflow.com/questions/13839865/how-to-parse-my-json-string-in-c4-0using-newtonsoft-json-package
        /// </summary>
        private T DeserializeJsonString<T>(string json) where T : class, new()
        {
            var jO = Newtonsoft.Json.Linq.JObject.Parse(json);
            return JsonConvert.DeserializeObject<T>(jO.ToString());
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

        /// <summary>
        /// Reads sprite from resources of specified file name. Null on failure.
        /// </summary>
        private Sprite ReadSprite(string filename)
        {
            var b = File.ReadAllBytes(filename);
            var t = new Texture2D(2, 2);//size will get replaced by incoming file
            t.LoadImage(b);

            var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2());
            if (sprite == null)
            {
                Debug.LogError("Cannot create sprite from " + filename);
            }
            return sprite;
        }
    }
}
