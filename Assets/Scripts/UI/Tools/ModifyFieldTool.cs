using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class ModifyFieldTool : ModifyTool
    {
        public InputFieldType _fieldType;
        public Text _currentValueText;
        public Text _placeholderText;
        public UnitType _unitType = UnitType.None;
        public string _optPlcHlderTxtPref = "";

        protected InputField _inputField;

        protected override void Awake()
        {
            base.Awake();

            UpdateUnits(EnvironManager.Instance.Environ.SimSettings.IsMetricUnits);
        }

        public void UpdateUnits(bool isMetric)
        {
            if (_unitType != UnitType.None)
            {
                string u = "";
                if (_unitType == UnitType.Distance)
                {
                    u = isMetric ? " (m)" : " (ft)";
                }
                else if (_unitType == UnitType.Speed)
                {
                    u = isMetric ? " (km/hr)" : " (mi/hr)";
                }
                _placeholderText.text = _optPlcHlderTxtPref + u;
            }
        }

        /// <summary>
        /// Sets value of "current value" text
        /// </summary>
        public void SetCurrentValue(String value)
        {
            _currentValueText.text = value;
        }

        protected override void Init()
        {
            _inputField = GetComponentInChildren<InputField>();
            if (_inputField == null)
            {
                Debug.LogError("Input field not found on field tool children");
            }
        }

        protected override IModifyElementArgs GatherInformation()
        {
            if (_inputField.text == "") return null;
            _currentValueText.text = _inputField.text;
            if (_propertyType == ElementPropertyType.Unset)
            {
                Debug.LogError("Modify field tool property is unset");
            }
            if (_fieldType == InputFieldType.Unset)
            {
                Debug.LogError("Modify field type is unset");
            }
            string t = _inputField.text;
            _inputField.SetTextWithoutNotify("");
            switch (_fieldType)
            {
                case InputFieldType.String_:
                    {
                        return new ModifyElementArgs(new ModifyStringPropertyArg(_propertyType, t));
                    }
                case InputFieldType.Integer_:
                    {
                        if (int.TryParse(t, out int i))
                        {
                            return new ModifyElementArgs(new ModifyIntPropertyArg(_propertyType, i));
                        }
                        else
                        {
                            Debug.Log("Wrong input format");
                            break;
                        }
                    }
                case InputFieldType.Float_:
                    {
                        if (float.TryParse(t, out float f))
                        {
                            return new ModifyElementArgs(new ModifyFloatPropertyArg(_propertyType, f));
                        }
                        else
                        {
                            Debug.Log("Wrong input format");
                            break;
                        }
                    }
            }

            return new ModifyElementArgs(new ModifyStringPropertyArg(ElementPropertyType.Unset, t));//just a placeholder
        }
    }
}
