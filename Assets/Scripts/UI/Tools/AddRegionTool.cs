using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class AddRegionTool : AddTool
    {
        protected override void Initialize()
        {
            
        }

        protected override IAddElementArgs GatherInformation()
        {
            return new AddRegionArgs(_position, _hitInfo);
        }
    }
}
