using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.EventArgs
{
    public interface IAddElementArgs
    {
        string Type { get; }
        Vector3 Position { get; }
    }
}
