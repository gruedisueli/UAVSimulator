using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HighlightElement : MonoBehaviour
    {
        public Image _image;//sprite for this highlight
        public double _flashRate = 1;//in seconds, for full cycle
        public float _maxOpacity = 1.0f;//max opacity allowed for image (0-1);
        public float _minOpacity = 0.0f;//min opacity allowed for image (0-1);
        private double _startTime = 0;//start time of cycle
        private float _opacityRange;

        private void Start()
        {
            _startTime = Time.unscaledTimeAsDouble;
            _opacityRange = _maxOpacity - _minOpacity;
        }
        private void Update()
        {
            double currentTime = Time.unscaledTimeAsDouble - _startTime;
            if (currentTime > _flashRate)
            {
                currentTime = 0;
                _startTime = Time.unscaledTimeAsDouble;
            }

            double currentPos = (currentTime / _flashRate) * Math.PI;
            float opacity = _maxOpacity - (float)Math.Sin(currentPos) * _opacityRange;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, opacity);
        }
    }
}
