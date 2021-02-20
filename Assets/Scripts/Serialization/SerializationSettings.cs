using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Serialization
{
    public static class SerializationSettings
    {
        public static readonly string ROOT = Application.dataPath;//should give you the "Assets" folder as root
        public static readonly string SAVE_PATH = ROOT + "/Saves/test.json"; //TEMPORARY UNTIL WE HAVE USER-SPECIFIED NAMING
    }
}
