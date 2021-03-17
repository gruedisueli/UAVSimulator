using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class PlayPauseTool : MonoBehaviour
    {
        public EventHandler<PlayPauseArgs> OnPlayPause;

        public void PlayPause()
        {
            OnPlayPause.Invoke(this, new PlayPauseArgs());
        }
    }
}
