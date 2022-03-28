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
        public Sprite _playImage;
        public Sprite _stopImage;
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
            _image.sprite = _isOn ? _stopImage : _playImage;
            OnPlayPause?.Invoke(this, new PlayPauseArgs(_isOn));
        }
    }
}
