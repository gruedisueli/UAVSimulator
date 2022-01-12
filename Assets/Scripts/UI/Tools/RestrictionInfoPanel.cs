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
            switch (_type)
            {
                case RestrictionPanelType.Rect:
                {
                    if (!(specs is RestrictionZoneRect rZ)) break;
                    _infoPanel.SetTextElement(ElementPropertyType.Rotation, rZ.Rotation.y);
                    _infoPanel.SetTextElement(ElementPropertyType.Height, rZ.Height);
                    _infoPanel.SetTextElement(ElementPropertyType.XScale, rZ.Scale.x);
                    _infoPanel.SetTextElement(ElementPropertyType.ZScale, rZ.Scale.z);
                    SetModifyToolValue(ElementPropertyType.Rotation, rZ.Rotation.y);
                    SetModifyToolValue(ElementPropertyType.Height, rZ.Height);
                    SetModifyToolValue(ElementPropertyType.XScale, rZ.Scale.x);
                    SetModifyToolValue(ElementPropertyType.ZScale, rZ.Scale.z);
                    break;
                }
                case RestrictionPanelType.Cyl:
                {
                    if (!(specs is RestrictionZoneCyl rZ)) break;
                    _infoPanel.SetTextElement(ElementPropertyType.Top, rZ.Top);
                    _infoPanel.SetTextElement(ElementPropertyType.Bottom, rZ.Bottom);
                    _infoPanel.SetTextElement(ElementPropertyType.Radius, rZ.Radius);
                    SetModifyToolValue(ElementPropertyType.Top, rZ.Top);
                    SetModifyToolValue(ElementPropertyType.Bottom, rZ.Bottom);
                    SetModifyToolValue(ElementPropertyType.Radius, rZ.Radius);
                    break;
                    }
                case RestrictionPanelType.CylStacked:
                {
                    break;
                }
            }
        }
    }
}
