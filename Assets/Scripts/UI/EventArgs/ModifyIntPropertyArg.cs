using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    /// <summary>
    /// A class to hold a single property update
    /// </summary>
    public class ModifyIntPropertyArg : ModifyPropertyArgBase, IModifyPropertyArg<int>
    {
        public override ToolMessageCategory Category { get; protected set; }
        public override VisibilityType VisibilityType { get; protected set; } = VisibilityType.Unset;
        public override ElementPropertyType ElementPropertyType { get; protected set; }
        public int Value { get; private set; }

        public ModifyIntPropertyArg(ElementPropertyType type, int value)
        {
            Category = ToolMessageCategory.ElementModification;
            ElementPropertyType = type;
            Value = value;
        }

        public ModifyIntPropertyArg(VisibilityType type, int value)
        {
            Category = ToolMessageCategory.VisibilityModification;
            VisibilityType = type;
            Value = value;
        }
    }
}
