using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public class SceneChangeArgs : System.EventArgs
    {
        public SceneType SceneType { get; private set; }

        public SceneChangeArgs(SceneType sceneType)
        {
            SceneType = sceneType;
        }
    }
}
