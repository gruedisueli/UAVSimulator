using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI
{
    public static class UISettings
    {
        public static readonly int MAINMENU_SCENEPATH = 0;
        public static readonly int FINDLOCATION_SCENEPATH = 1;
        public static readonly int REGIONVIEW_SCENEPATH = 2;
        public static readonly int CITYVIEW_SCENEPATH = 3;

        public static readonly float REGIONVIEW_NETWORK_WIDTH = 100.0f;
        public static readonly float CITYVIEW_NETWORK_WIDTH = 5.0f;
    }
}
