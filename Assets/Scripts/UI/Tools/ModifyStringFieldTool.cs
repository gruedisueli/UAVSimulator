using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class ModifyStringFieldTool : ModifyFieldTool
    {
        protected override IModifyElementArgs GatherInformation()
        {
            if (_propertyType == ElementPropertyType.Unset)
            {
                Debug.LogError("Modify field tool property is unset");
            }
            string t = _inputField.text;
            _inputField.text = "";
            return new ModifyElementArgs(new ModifyStringPropertyArg(_propertyType, t));
        }
    }
}
