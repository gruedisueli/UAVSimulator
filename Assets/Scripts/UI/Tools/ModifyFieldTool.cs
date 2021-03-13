using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class ModifyFieldTool : ModifyTool
    {
        public InputFieldType _fieldType;

        protected InputField _inputField;

        protected override void Init()
        {
            _inputField = GetComponent<InputField>();
            if (_inputField == null)
            {
                Debug.LogError("Input field not found on field tool");
            }
        }

        protected override IModifyElementArgs GatherInformation()
        {
            if (_propertyType == ElementPropertyType.Unset)
            {
                Debug.LogError("Modify field tool property is unset");
            }
            if (_fieldType == InputFieldType.Unset)
            {
                Debug.LogError("Modify field type is unset");
            }
            string t = _inputField.text;
            _inputField.text = "";
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
