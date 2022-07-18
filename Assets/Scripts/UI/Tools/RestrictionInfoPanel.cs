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
        public RestrictionPanelType _type;
        private RestrictionZoneBase _zoneSpecs;
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
            _infoPanel.SetTextElement(ElementPropertyType.Type, t);
            _infoPanel.SetTextElement(ElementPropertyType.Description, specs.Description);

            UpdateFields(specs);
        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {

        }

        protected override void StartModify(object sender, System.EventArgs args)
        {
            UpdateFields(_zoneSpecs);
            base.StartModify(sender, args);
        }

        /// <summary>
        /// Refreshes fields on this panel
        /// </summary>
        public void UpdateFields(RestrictionZoneBase specs)
        {
            _zoneSpecs = specs;
            bool isMetric = EnvironManager.Instance.Environ.SimSettings.IsMetricUnits;
            switch (_type)
            {
                case RestrictionPanelType.Rect:
                {
                    if (!(specs is RestrictionZoneRect rZ)) break;
                    float h, sX, sZ;
                    if (isMetric)
                    {
                        h = rZ.Height;
                        sX = rZ.Scale.x;
                        sZ = rZ.Scale.z;
                    }
                    else
                    {
                        h = UnitUtils.MetersToFeet(rZ.Height);
                        sX = UnitUtils.MetersToFeet(rZ.Scale.x);
                        sZ = UnitUtils.MetersToFeet(rZ.Scale.z);
                    }
                    _infoPanel.SetTextElement(ElementPropertyType.Rotation, rZ.Rotation.y);
                    _infoPanel.SetTextElement(ElementPropertyType.Height, h);
                    _infoPanel.SetTextElement(ElementPropertyType.XScale, sX);
                    _infoPanel.SetTextElement(ElementPropertyType.ZScale, sZ);
                    SetModifyToolValue(ElementPropertyType.Rotation, rZ.Rotation.y);
                    SetModifyToolValue(ElementPropertyType.Height, h);
                    SetModifyToolValue(ElementPropertyType.XScale, sX);
                    SetModifyToolValue(ElementPropertyType.ZScale, sZ);
                    break;
                }
                case RestrictionPanelType.Cyl:
                {
                    if (!(specs is RestrictionZoneCyl rZ)) break;
                    float t, b, r;
                    if (isMetric)
                    {
                        t = rZ.Top;
                        b = rZ.Bottom;
                        r = rZ.Radius;
                    }
                    else
                    {
                        t = UnitUtils.MetersToFeet(rZ.Top);
                        b = UnitUtils.MetersToFeet(rZ.Bottom);
                        r = UnitUtils.MetersToFeet(rZ.Radius);
                    }
                    _infoPanel.SetTextElement(ElementPropertyType.Top, t);
                    _infoPanel.SetTextElement(ElementPropertyType.Bottom, b);
                    _infoPanel.SetTextElement(ElementPropertyType.Radius, r);
                    SetModifyToolValue(ElementPropertyType.Top, t);
                    SetModifyToolValue(ElementPropertyType.Bottom, b);
                    SetModifyToolValue(ElementPropertyType.Radius, r);
                    break;
                }
            }
        }
    }
}
