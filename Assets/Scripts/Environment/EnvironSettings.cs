using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Mapbox.Utils;

namespace Assets.Scripts.Environment
{
    public static class EnvironSettings
    {
        public static readonly int CITY_ZOOM_LEVEL = 15;
        public static readonly int REGION_ZOOM_LEVEL = 10;
        public static readonly int TILE_RESOLUTION = 256;
        public static readonly double SCALE_CONSTANT = 156543.03; //meters/pixel, see https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale
    }
}
