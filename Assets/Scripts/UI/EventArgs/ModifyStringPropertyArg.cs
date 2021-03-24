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
    public class ModifyStringPropertyArg : ModifyPropertyArgBase, IModifyPropertyArg<string>
    {
        public override ToolMessageCategory Category { get; protected set; }
        public override VisibilityType VisibilityType { get; protected set; } = VisibilityType.Unset;
        public override ElementPropertyType ElementPropertyType { get; protected set; }
        public string Value { get; private set; }

        public ModifyStringPropertyArg(ElementPropertyType type, string value)
        {
            Category = ToolMessageCategory.ElementModification;
            ElementPropertyType = type;
            Value = value;
        }

        public ModifyStringPropertyArg(VisibilityType type, string value)
        {
            Category = ToolMessageCategory.VisibilityModification;
            VisibilityType = type;
            Value = value;
        }
    }
}
