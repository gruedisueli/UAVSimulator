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
        public static readonly int MEDIUM_ZOOM_LEVEL = 11;
        public static readonly int REGION_ZOOM_LEVEL = 9;
        public static readonly int FINDLOCATION_ZOOM_LEVEL = 7;
        public static readonly int TILE_RESOLUTION = 256;
        public static readonly double SCALE_CONSTANT = 156543.03; //meters/pixel, see https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale
        public static readonly int REGION_TILE_EXTENTS = 7;//how many tiles n/s/e/w of center to build in region view.
        public static readonly float FEET_PER_METERS = 3.28084f;
        public static readonly float FEET_PER_MILE = 5280.0f;
        public static readonly float SECONDS_PER_HOUR = 3600.0f;
    }
}
