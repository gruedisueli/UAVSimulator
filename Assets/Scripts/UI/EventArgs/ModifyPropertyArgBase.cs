using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public abstract class ModifyPropertyArgBase
    {
        public abstract ToolMessageCategory Category { get; protected set; }
        public abstract ElementPropertyType ElementPropertyType { get; protected set; } 
        public abstract VisibilityType VisibilityType { get; protected set; }
    }
}
