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
    public class UpdateIntPropertyArg : UpdatePropertyArgBase, IUpdatePropertyArg<int>
    {
        public override UpdatePropertyType Type { get; protected set; }
        public int Value { get; private set; }
        public UpdateIntPropertyArg(UpdatePropertyType type, int value)
        {
            Type = type;
            Value = value;
        }
    }
}
