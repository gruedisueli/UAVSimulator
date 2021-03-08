using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Events
{
    /// <summary>
    /// A class to hold a single property update
    /// </summary>
    public class UpdateFloatPropertyArg : UpdatePropertyArgBase, IUpdatePropertyArg<float>
    {
        public UpdatePropertyType Type { get; private set; }
        public float Value { get; private set; }
        public UpdateFloatPropertyArg(UpdatePropertyType type, float value)
        {
            Type = type;
            Value = value;
        }
    }
}
