using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class SceneChangeTool : MonoBehaviour
    {
        public SceneType _sceneType;
        public event EventHandler<SceneChangeArgs> OnSceneChange;

        public void ChangeScene()
        {
            OnSceneChange.Invoke(this, new SceneChangeArgs(_sceneType));
        }
    }
}
