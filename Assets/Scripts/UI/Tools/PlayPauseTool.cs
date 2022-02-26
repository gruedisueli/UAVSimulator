using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class PlayPauseTool : ToolBase
    {
        public EventHandler<PlayPauseArgs> OnPlayPause;
        private Toggle _toggle;
        private Image _image;

        void Awake()
        {
            _toggle = gameObject.GetComponent<Toggle>();
            if (_toggle == null)
            {
                Debug.LogError("Could not find toggle component on play pause button");
            }

            _image = gameObject.GetComponent<Image>();
            if (_image == null)
            {
                Debug.LogError("Could not find image component on play pause button");
            }
        }
        public void PlayPause()
        {
            _image.color = _toggle.isOn ? Color.blue : Color.white;
            OnPlayPause?.Invoke(this, new PlayPauseArgs(_toggle.isOn));
        }
    }
}
