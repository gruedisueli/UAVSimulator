using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.Environment
{
    public abstract class RestrictionZoneBase
    {
        public int Layer { get; set; } = 8;
        public abstract string Type { get; }
        public abstract string Description { get; }

        public abstract Vector3 Position { get; set; }
        public abstract void UpdateParams(ModifyPropertyArgBase args);
        public abstract RestrictionZoneBase GetCopy();

        /// <summary>
        /// Builds boundary points at specified height, projecting boundary points onto this height value. Inflated by specified amount
        /// </summary>
        public abstract List<Vector3> GetBoundaryPtsAtHeight(float height, float inflation);
    }
}
