using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public abstract class RestrictionZoneBase
    {
        public int Layer { get; } = 8;
    }
}
