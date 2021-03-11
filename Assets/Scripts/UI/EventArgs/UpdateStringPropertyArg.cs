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
    public class UpdateStringPropertyArg : UpdatePropertyArgBase, IUpdatePropertyArg<string>
    {
        public override UpdatePropertyType Type { get; protected set; }
        public string Value { get; private set; }
        public UpdateStringPropertyArg(UpdatePropertyType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
