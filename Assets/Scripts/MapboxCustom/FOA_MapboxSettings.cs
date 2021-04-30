using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Assets.Scripts.MapboxCustom
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FOA_MapboxSettings
    {
        [JsonProperty]
        public string AccessToken = "";

        [JsonProperty]
        public int MemoryCacheSize = 0;

        [JsonProperty]
        public int FileCacheSize = 0;

        [JsonProperty]
        public int DefaultTimeOut = 0;

        [JsonProperty]
        public bool AutoRefreshCache = false;
    }
}
