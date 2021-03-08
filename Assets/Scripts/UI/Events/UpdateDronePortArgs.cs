using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Events
{
    public class UpdateDronePortArgs : IUpdateElementArgs
    {
        public UpdateElementType UpdateType { get; private set; } 
        public string Guid { get; set; }
        public UpdatePropertyArgBase Update { get; set; } = null;

        public UpdateDronePortArgs(UpdateElementType type)
        {
            UpdateType = type;
        }
    }
}
