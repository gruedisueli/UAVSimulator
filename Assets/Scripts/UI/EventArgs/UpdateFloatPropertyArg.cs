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
    public class UpdateFloatPropertyArg : UpdatePropertyArgBase, IUpdatePropertyArg<float>
    {
        public override ElementPropertyType Type { get; protected set; }
        public float Value { get; private set; }
        public UpdateFloatPropertyArg(ElementPropertyType type, float value)
        {
            Type = type;
            Value = value;
        }
    }
}
