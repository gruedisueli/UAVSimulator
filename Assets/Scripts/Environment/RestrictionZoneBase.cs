using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public abstract class RestrictionZoneBase
    {
        public int Layer { get; } = 8;
        public abstract void UpdateParams(ModifyPropertyArgBase args);
        public abstract RestrictionZoneBase GetCopy();
    }
}
