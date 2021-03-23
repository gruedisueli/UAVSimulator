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
    public class ParkingInfoPanel : ElementInfoPanel
    {
        public override void Initialize(SceneElementBase sceneElement)
        {
            base.Initialize(sceneElement);

            var sPark = sceneElement as SceneParkingStructure;
            if (sPark == null)
            {
                Debug.LogError("Provided scene element for parking structure info panel is not a scene parking structure");
                return;
            }

            var specs = sPark.ParkingStructureSpecs;
            string t = specs.Type;
            var asset = EnvironManager.Instance.ParkingStructAssets[t];
            _image.sprite = asset.PreviewImage;

            SetTextElement(ElementPropertyType.Type, t);
            SetTextElement(ElementPropertyType.Description, specs.Description);
            SetTextElement(ElementPropertyType.Rotation, specs.Rotation.y.ToString("F2"));

            SliderTool sT = GetComponentInChildren<SliderTool>(true);
            if (sT == null)
            {
                Debug.LogError("Slider Tool not found in children of parking input panel");
                return;
            }
            sT.SetValue(specs.Rotation.y);
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
                Debug.LogError("Casting error in parking structure modify update");
                return;
            }
        }
    }
}
