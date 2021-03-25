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
            _infoPanel.SetTextElement(ElementPropertyType.Type, t);
            _infoPanel.SetTextElement(ElementPropertyType.Description, specs.Description);
            _infoPanel.SetTextElement(ElementPropertyType.Rotation, specs.Rotation.y.ToString("F2"));
            _infoPanel.SetTextElement(ElementPropertyType.MaxVehicleSize, specs.MaximumVehicleSize.ToString("F2"));
            _infoPanel.SetTextElement(ElementPropertyType.XScale, specs.Scale.x.ToString("F2"));
            _infoPanel.SetTextElement(ElementPropertyType.ZScale, specs.Scale.z.ToString("F2"));
            

            SliderTool[] sliders = GetComponentsInChildren<SliderTool>(true);
            if (sliders == null || sliders.Length == 0)
            {
                Debug.LogError("Slider Tool not found in children of drone port input panel");
                return;
            }
            foreach (var sT in sliders)
            {
                bool scaleActive = specs is DronePortRect ? true : false;
                switch (sT._propertyType)
                {
                    case ElementPropertyType.Rotation:
                        {
                            sT.SetValue(specs.Rotation.y);
                            break;
                        }
                    case ElementPropertyType.XScale:
                        {
                            sT.SetValue(specs.Scale.x);
                            sT.SetInteractable(scaleActive);
                            break;
                        }
                    case ElementPropertyType.ZScale:
                        {
                            sT.SetValue(specs.Scale.z);
                            sT.SetInteractable(scaleActive);
                            break;
                        }
                }

            }

        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {
            try
            {
                switch (args.Update.ElementPropertyType)
                {
                    case ElementPropertyType.Rotation:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.Rotation, (args.Update as ModifyVector3PropertyArg).Value.y.ToString("F2"));
                            break;
                        }
                    case ElementPropertyType.XScale:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.XScale, (args.Update as ModifyFloatPropertyArg).Value.ToString("F2"));
                            break;
                        }
                    case ElementPropertyType.ZScale:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.ZScale, (args.Update as ModifyFloatPropertyArg).Value.ToString("F2"));
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
