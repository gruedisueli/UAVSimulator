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
    public class UpdateVector3PropertyArg : UpdatePropertyArgBase, IUpdatePropertyArg<Vector3>
    {
        public override ElementPropertyType Type { get; protected set; }
        public Vector3 Value { get; private set; }
        public UpdateVector3PropertyArg(ElementPropertyType type, Vector3 value)
        {
            Type = type;
            Value = value;
        }
    }
}
