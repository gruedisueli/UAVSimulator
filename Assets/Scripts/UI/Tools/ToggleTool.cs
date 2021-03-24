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
    public class ToggleTool : ModifyTool
    {
        public bool _isOn;

        protected Toggle _toggle;
        protected Text _text;

        protected override void Awake()
        {
            base.Awake();

            _toggle = GetComponentInChildren<Toggle>(true);
            if (_toggle == null)
            {
                Debug.LogError("Toggle Component not found in children of Toggle Tool");
                return;
            }

            _text = GetComponentInChildren<Text>(true);
            if (_text == null)
            {
                Debug.LogError("Text Component not found in children of Toggle Tool");
                return;
            }

            _text.text = _isVisibilityModifier ? _visibilityType.ToString() : _propertyType.ToString();
            _toggle.isOn = _isOn;
            _toggle.onValueChanged.AddListener(ValueChanged);
        }

        protected override void Init()
        {
            
        }

        protected void ValueChanged(bool t)
        {
            _isOn = t;
            RegisterModification();
        }

        protected override IModifyElementArgs GatherInformation()
        {
            if (_propertyType != ElementPropertyType.Unset)
            {
                return new ModifyElementArgs(new ModifyBoolPropertyArg(_propertyType, _isOn));
            }
            else if (_visibilityType != VisibilityType.Unset)
            {
                return new ModifyElementArgs(new ModifyBoolPropertyArg(_visibilityType, _isOn));
            }

            return new ModifyElementArgs(new ModifyBoolPropertyArg(ElementPropertyType.Unset, _isOn));
        }
    }
}
