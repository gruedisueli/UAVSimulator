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
        public override ElementPropertyType Type { get; protected set; }
        public string Value { get; private set; }
        public ModifyStringPropertyArg(ElementPropertyType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
