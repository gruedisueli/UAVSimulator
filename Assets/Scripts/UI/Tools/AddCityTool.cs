using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class AddCityTool : AddTool
    {


        protected override void Initialize()
        {
            
        }

        protected override IAddElementArgs GatherInformation()
        {
            return new AddCityArgs(_position, _hitInfo);
        }

        
    }
}
