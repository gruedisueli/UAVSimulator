using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class DronePortInfoPanel : ElementInfoPanel
    {
        public override void Initialize(SceneElementBase sceneElement)
        {
            base.Initialize(sceneElement);

            var sceneDP = sceneElement as SceneDronePort;
            if (sceneDP == null)
            {
                Debug.LogError("Provided scene element for drone port info panel is not a drone port");
                return;
            }

            var specs = sceneDP.DronePortSpecs;
            string t = specs.Type;
            var asset = EnvironManager.Instance.DronePortAssets[t];
            _image.sprite = asset.PreviewImage;
            SetTextElement(ElementPropertyType.Type, t);
            SetTextElement(ElementPropertyType.Description, specs.Description);
            SetTextElement(ElementPropertyType.Rotation, specs.Rotation.y.ToString("F2"));
            SetTextElement(ElementPropertyType.MaxVehicleSize, specs.MaximumVehicleSize.ToString("F2"));

            SliderTool sT = GetComponentInChildren<SliderTool>(true);
            if (sT == null)
            {
                Debug.LogError("Slider Tool not found in children of drone port input panel");
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
                Debug.LogError("Casting error in drone port modify update");
                return;
            }
        }
    }
}
