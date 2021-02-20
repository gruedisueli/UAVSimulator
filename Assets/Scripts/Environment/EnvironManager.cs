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
        public Dictionary<string, DronePort> DronePortSpecs { get; private set; } = new Dictionary<string, DronePort>();
        public Dictionary<string, List<Vector3>> ParkingStructSpecs { get; private set; } = new Dictionary<string, List<Vector3>>();


        /// <summary>
        /// Create an entirely new environment.
        /// </summary>
        public void CreateNew()
        {
            Environ = new Environ();
            ReadDronePortSpecs();
            ReadParkingStructureSpecs();
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
        private void ReadParkingStructureSpecs()
        {
            ParkingStructSpecs = new Dictionary<string, List<Vector3>>();

            string path = SerializationSettings.ROOT + "\\Resources\\ParkingStructures";
            var files = Directory.GetFiles(path, "*.DAT");

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
                ParkingStructSpecs.Add(type, spots);
            }

        }

        /// <summary>
        /// Reads types of drone ports from assets.
        /// </summary>
        private void ReadDronePortSpecs()
        {
            DronePortSpecs = new Dictionary<string, DronePort>();

            string path = SerializationSettings.ROOT + "\\Resources\\DronePorts";
            var files = Directory.GetFiles(path, "*.JSON");
            foreach (var filename in files)
            {
                string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
                DronePort dp = JsonUtility.FromJson<DronePort>(json);
                DronePortSpecs.Add(dp.type, dp);
            }
        }
    }
}
