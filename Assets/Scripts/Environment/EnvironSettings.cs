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
        public static readonly int FINDLOCATION_SELECTION_INFLATION = 3; //how many tiles n/s/e/w of selected tile to highlight...all regions are square for simplicity
        public static readonly int FINDLOCATION_ZOOM_LEVEL = 9;
        public static readonly int AIRSPACE_ZOOM_LEVEL = 6;
        public static readonly int TILE_RESOLUTION = 256;
        public static readonly double SCALE_CONSTANT = 156543.03; //meters/pixel, see https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale
        public static readonly float RANGE_LIMIT = 4000.0f;
        public static readonly int REGION_TILE_EXTENTS = 7;//how many tiles n/s/e/w of center to build in region view.
        //public static readonly int REGION_TILE_MAX = 225;
    }
}
