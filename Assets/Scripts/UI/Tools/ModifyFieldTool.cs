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
    public abstract class ModifyFieldTool : ModifyTool
    {
        protected InputField _inputField;

        protected override void Init()
        {
            _inputField = GetComponent<InputField>();
            if (_inputField == null)
            {
                Debug.LogError("Input field not found on field tool");
            }
        }
    }
}
