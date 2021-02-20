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
    public sealed class EnvironController
    {
        private static EnvironController _instance = null;
        private static readonly object _lock = new object();

        private EnvironController()
        {
            CreateNew();
        }

        public static EnvironController Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EnvironController();
                    }
                    return _instance;
                }
            }
        }

        public Environ Environ { get; private set; } = new Environ();
        public string ActiveCity { get; private set; } = "";

        /// <summary>
        /// Create an entirely new environment.
        /// </summary>
        public void CreateNew()
        {
            Environ = new Environ();
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
    }
}
