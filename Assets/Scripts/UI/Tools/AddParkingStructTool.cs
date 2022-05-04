using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class AddParkingStructTool : AddTool
    {
        public ParkingStructCategory _category;
        public string _type = "";

        protected override void Initialize()
        {
            
        }

        protected override void ActivateMouseHint()
        {
            EnvironManager.Instance.CanvasMouseHint.Activate("Click to place a parking structure");
        }

        protected override IAddElementArgs GatherInformation()
        {
            if (_category == ParkingStructCategory.Unset)
            {
                Debug.LogError("No parking structure category selected");
            }
            if (_type == "")
            {
                Debug.LogError("No parking structure type selected");
            }
            return new AddParkingStructArgs(_category, _type, _position);
        }

        
    }
}
