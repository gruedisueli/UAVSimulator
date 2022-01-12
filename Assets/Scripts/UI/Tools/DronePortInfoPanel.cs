using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts.UI.Tools
{
    public class DronePortInfoPanel : ElementInfoPanel
    {
        private DronePortBase _portSpecs;

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
            _infoPanel.SetTextElement(ElementPropertyType.MaxVehicleSize, specs.MaximumVehicleSize.ToString("F2"));

            UpdateFields(specs);
        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {

        }

        protected override void StartModify(object sender, System.EventArgs args)
        {
            UpdateFields(_portSpecs);
            base.StartModify(sender, args);
        }

        /// <summary>
        /// Refreshes fields on this panel
        /// </summary>
        public void UpdateFields(DronePortBase specs)
        {
            _portSpecs = specs;
            _infoPanel.SetTextElement(ElementPropertyType.Rotation, specs.Rotation.y);
            SetModifyToolValue(ElementPropertyType.Rotation, specs.Rotation.y);
        }
    }
}
