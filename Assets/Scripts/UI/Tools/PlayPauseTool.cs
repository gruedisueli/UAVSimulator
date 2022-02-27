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
        public bool _isOn = false;
        private Image _image;

        void Awake()
        {
            _image = gameObject.GetComponent<Image>();
            if (_image == null)
            {
                Debug.LogError("Could not find image component on play pause button");
            }
        }
        public void PlayPause()
        {
            _isOn = !_isOn;
            _image.color = _isOn ? Color.blue : Color.white;
            OnPlayPause?.Invoke(this, new PlayPauseArgs(_isOn));
        }
    }
}
