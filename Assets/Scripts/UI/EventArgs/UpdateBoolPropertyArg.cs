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
    public class UpdateBoolPropertyArg : UpdatePropertyArgBase, IUpdatePropertyArg<bool>
    {
        public override ElementPropertyType Type { get; protected set; }
        public bool Value { get; private set; }
        public UpdateBoolPropertyArg(ElementPropertyType type, bool value)
        {
            Type = type;
            Value = value;
        }
    }
}
