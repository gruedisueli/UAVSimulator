using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    /// <summary>
    /// A class to hold a single property update
    /// </summary>
    public class ModifyVector3PropertyArg : ModifyPropertyArgBase, IModifyPropertyArg<Vector3>
    {
        public override ToolMessageCategory Category { get; protected set; }
        public override VisibilityType VisibilityType { get; protected set; } = VisibilityType.Unset;
        public override ElementPropertyType ElementPropertyType { get; protected set; }
        public Vector3 Value { get; private set; }

        public ModifyVector3PropertyArg(ElementPropertyType type, Vector3 value)
        {
            Category = ToolMessageCategory.ElementModification;
            ElementPropertyType = type;
            Value = value;
        }

        public ModifyVector3PropertyArg(VisibilityType type, Vector3 value)
        {
            Category = ToolMessageCategory.VisibilityModification;
            VisibilityType = type;
            Value = value;
        }
    }
}
