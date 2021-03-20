using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.Environment;

namespace Assets.Scripts.UI.Tools
{
    public class RestrictionInfoPanel : ElementInfoPanel
    {
        public override void Initialize(SceneElementBase sceneElement)
        {
            base.Initialize(sceneElement);

            var sRest = sceneElement as SceneRestrictionZone;
            if (sRest == null)
            {
                Debug.LogError("Provided scene element for restrction zone info panel is not a scene restriction zone");
                return;
            }

            var specs = sRest.RestrictionZoneSpecs;
            string t = specs 

                //different types of restriction zones mean params may be different from one to next.
        }
    }
}
