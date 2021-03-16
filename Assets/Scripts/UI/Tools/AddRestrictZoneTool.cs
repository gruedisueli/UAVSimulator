using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class AddRestrictZoneTool : AddTool
    {
        public RestrictionZoneCategory _category;
        public string _type = "";

        protected override void Initialize()
        {
            
        }

        protected override IAddElementArgs GatherInformation()
        {
            if (_category == RestrictionZoneCategory.Unset)
            {
                Debug.LogError("No restriction zone category selected");
            }
            if (_type == "")
            {
                Debug.LogError("No restriction zone type selected");
            }
            return new AddRestrictionZoneArgs(_category, _type, _position);
        }

        
    }
}
