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
        public ParkingPanelType _type;
        private ParkingStructureBase _parkingSpecs;

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
            string simpleName = t.Contains("LowAltitude") ? "Low Altitude Parking" : "Corridor Parking";
            var asset = EnvironManager.Instance.ParkingStructAssets[t];
            _image.sprite = asset.PreviewImage;

            _infoPanel.SetTextElement(ElementPropertyType.Type, simpleName);
            _infoPanel.SetTextElement(ElementPropertyType.Description, specs.Description);

            UpdateFields(specs);
        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {

        }

        protected override void StartModify(object sender, System.EventArgs args)
        {
            UpdateFields(_parkingSpecs);
            base.StartModify(sender, args);
        }

        /// <summary>
        /// Refreshes fields on this panel
        /// </summary>
        public void UpdateFields(ParkingStructureBase specs)
        {
            _parkingSpecs = specs;
            bool isMetric = EnvironManager.Instance.Environ.SimSettings.IsMetricUnits;
            switch (_type)
            {
                case ParkingPanelType.Rect:
                {
                    if (!(specs is ParkingStructureRect pS)) break;
                    float sX, sZ;
                    if (isMetric)
                    {
                        sX = pS.Scale.x;
                        sZ = pS.Scale.z;
                    }
                    else
                    {
                        sX = UnitUtils.MetersToFeet(pS.Scale.x);
                        sZ = UnitUtils.MetersToFeet(pS.Scale.z);
                    }
                    _infoPanel.SetTextElement(ElementPropertyType.Rotation, pS.Rotation.y);
                    _infoPanel.SetTextElement(ElementPropertyType.XScale, sX);
                    _infoPanel.SetTextElement(ElementPropertyType.ZScale, sZ);
                    SetModifyToolValue(ElementPropertyType.Rotation, pS.Rotation.y);
                    SetModifyToolValue(ElementPropertyType.XScale, sX);
                    SetModifyToolValue(ElementPropertyType.ZScale, sZ);
                    SetModifyToolValue(ElementPropertyType.DroneInstantiationCt, pS.GetDroneInstantiationCt());
                    break;
                }
                case ParkingPanelType.Custom:
                {
                    if (!(specs is ParkingStructureCustom pS)) break;
                    _infoPanel.SetTextElement(ElementPropertyType.Rotation, pS.Rotation.y);
                    SetModifyToolValue(ElementPropertyType.Rotation, pS.Rotation.y);
                    SetModifyToolValue(ElementPropertyType.DroneInstantiationCt, pS.GetDroneInstantiationCt());
                        break;
                }
            }
        }
    }
}
