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
    public class ModifyFloatPropertyArg : ModifyPropertyArgBase, IModifyPropertyArg<float>
    {
        public override ToolMessageCategory Category { get; protected set; }
        public override VisibilityType VisibilityType { get; protected set; } = VisibilityType.Unset;
        public override ElementPropertyType ElementPropertyType { get; protected set; } = ElementPropertyType.Unset;
        public float Value { get; private set; }

        public ModifyFloatPropertyArg(ElementPropertyType type, float value)
        {
            Category = ToolMessageCategory.ElementModification;
            ElementPropertyType = type;
            Value = value;
        }

        public ModifyFloatPropertyArg(VisibilityType type, float value)
        {
            Category = ToolMessageCategory.VisibilityModification;
            VisibilityType = type;
            Value = value;
        }
    }
}
