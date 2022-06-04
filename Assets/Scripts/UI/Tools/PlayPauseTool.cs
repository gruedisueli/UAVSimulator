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
        public ToolTip _toolTip;
        private Image _image;
        private Button _button;

        void Awake()
        {
            _image = gameObject.GetComponent<Image>();
            if (_image == null)
            {
                Debug.LogError("Could not find image component on play pause button");
            }

            _button = GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("Could not find button on play pause tool");
            }

            _toolTip = GetComponentInChildren<ToolTip>(true);
            if (_toolTip == null)
            {
                Debug.LogError("Could not find tooltip in children of play pause tool");
            }
        }
        public void PlayPause()
        {
            _isOn = !_isOn;
            _image.sprite = _isOn ? _stopImage : _playImage;
            OnPlayPause?.Invoke(this, new PlayPauseArgs(_isOn));
        }

        public void SetIsInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        public bool GetIsInteractable()
        {
            return _button.interactable;
        }
    }
}
