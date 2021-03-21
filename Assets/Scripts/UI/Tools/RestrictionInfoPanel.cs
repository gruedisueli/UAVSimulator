using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

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
            string t = specs.Type;
            var asset = EnvironManager.Instance.RestrictionZoneAssets[t];
            _image.sprite = asset.PreviewImage;
            SetTextElement(ElementPropertyType.Type, t);
            SetTextElement(ElementPropertyType.Description, specs.Description);

            //not done building out this panel.

            //if (specs is RestrictionZoneRect)
            //{
            //    var s = specs as RestrictionZoneRect;
            //    SetTextElement(ElementPropertyType.Rotation, s.Rotation.y.ToString("F2"));
            //}
            //else if (specs is RestrictionZoneCyl)
            //{
            //    var s = specs as RestrictionZoneCyl;
            //    SetTextElement(ElementPropertyType.)
            //}
            //else if (specs is RestrictionZoneCylStack)
            //{

            //}

                //different types of restriction zones mean params may be different from one to next.
        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {
            try
            {
                switch (args.Update.Type)
                {
                    case ElementPropertyType.Rotation:
                        {
                            SetTextElement(ElementPropertyType.Rotation, (args.Update as ModifyFloatPropertyArg).Value.ToString("F2"));
                            break;
                        }
                }

            }
            catch
            {
                Debug.LogError("Casting error in restriction zone modify update");
                return;
            }
        }
    }
}
