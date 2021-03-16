using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class AddDronePortTool : AddTool
    {
        public DronePortCategory _category;
        public string _type = "";

        protected override void Initialize()
        {
            
        }

        protected override IAddElementArgs GatherInformation()
        {
            if (_category == DronePortCategory.Unset)
            {
                Debug.LogError("No drone port category selected");
            }
            if (_type == "")
            {
                Debug.LogError("No drone port type selected");
            }
            return new AddDronePortArgs(_category, _type, _position);
        }

        
    }
}
