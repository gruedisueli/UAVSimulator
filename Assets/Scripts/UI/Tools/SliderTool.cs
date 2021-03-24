using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.EventArgs;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// For managing flow of information from a slider
    /// </summary>
    public class SliderTool : ModifyTool
    {
        protected Slider _slider;
        protected Text _text;
        protected float _value = 0;

        protected override void Awake()
        {
            base.Awake();

            _slider = GetComponentInChildren<Slider>(true);
            if (_slider == null)
            {
                Debug.LogError("Slider Component not found in children of Slider Tool");
                return;
            }

            _text = GetComponentInChildren<Text>(true);
            if (_text == null)
            {
                Debug.LogError("Text Component not found in children of Slider Tool");
                return;
            }

            _text.text = _isVisibilityModifier ? _visibilityType.ToString() : _propertyType.ToString() + ": " + _slider.value.ToString("F2");

            _slider.onValueChanged.AddListener(ValueChanged);
        }

        protected override void Init()
        {
            
        }

        /// <summary>
        /// Allows setting of value remotely, such as when starting up.
        /// </summary>
        public void SetValue(float val)
        {
            _value = val;
            _slider.value = val;
        }

        protected void ValueChanged(float f)
        {
            _text.text = _propertyType.ToString() + ": " + f.ToString("F2");
            _value = f;
            RegisterModification();
        }

        protected override IModifyElementArgs GatherInformation()
        {
            switch (_propertyType)
            {
                case ElementPropertyType.Rotation:
                    {
                        return new ModifyElementArgs(new ModifyVector3PropertyArg(_propertyType, new Vector3(0, _value, 0)));
                    }

            }
            
            switch (_visibilityType)
            {
                case VisibilityType.DroneCount:
                    {
                        return new ModifyElementArgs(new ModifyIntPropertyArg(_visibilityType, (int)_value));
                    }
            }

            return new ModifyElementArgs(new ModifyFloatPropertyArg(_propertyType, _value));
        }
    }
}
