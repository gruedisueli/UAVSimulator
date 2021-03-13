using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

namespace Assets.Scripts
{
    public static class AssetUtils
    {
        /// <summary>
        /// Reads prefab from resources of specified file name. Will load .prefab or .obj files. Null on failure.
        /// </summary>
        public static GameObject ReadPrefab(string resourcePath, string name)
        {
            var pfb = Resources.Load<GameObject>(resourcePath + name);
            if (pfb == null)
            {
                Debug.LogError("Cannot find prefab for " + name);
            }
            return pfb;
        }

        /// <summary>
        /// Returns asset of specified type from json.
        /// </summary>
        public static T ReadJsonAsset<T>(string fileName)
        {
            string json = File.ReadAllText(fileName, Encoding.UTF8);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
