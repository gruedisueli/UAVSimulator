using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.Fields;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// A panel that contains tools for modifying a city. Doesn't modify the city itself. There are child objects for this.
    /// </summary>
    public class CityInfoPanel : ElementInfoPanel
    {
        public SceneChangeTool GoToCity { get; private set; }
        private CityOptions _cityOptions;

        public override void Initialize(SceneElementBase sceneElement)
        {
            base.Initialize(sceneElement);

            GoToCity = FindCompInChildren<SceneChangeTool>();

            var sCity = sceneElement as SceneCity;
            if (sCity == null)
            {
                Debug.LogError("Provided scene element for city info panel is not a scene city");
                return;
            }

            _cityOptions = sCity.CitySpecs;
            UpdateFields(_cityOptions);
        }

        protected override void StartModify(object sender, System.EventArgs args)
        {
            UpdateFields(_cityOptions);
            base.StartModify(sender, args);

            GoToCity.SetInteractable(false);
        }

        protected override void CommitModify(object sender, System.EventArgs args)
        {
            base.CommitModify(sender, args);

            GoToCity.SetInteractable(true);
        }

        protected override void CancelModify(object sender, System.EventArgs args)
        {
            base.CancelModify(sender, args);

            GoToCity.SetInteractable(true);
        }

        public void UpdateFields(CityOptions specs)
        {
            _cityOptions = specs;
            _infoPanel.SetTextElement(ElementPropertyType.Name, specs.Name);
            _infoPanel.SetTextElement(ElementPropertyType.EastExt, specs.EastExt.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.WestExt, specs.WestExt.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.NorthExt, specs.NorthExt.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.SouthExt, specs.SouthExt.ToString());
            SetModifyToolValue(ElementPropertyType.Name, specs.Name);
            SetModifyToolValue(ElementPropertyType.EastExt, specs.EastExt);
            SetModifyToolValue(ElementPropertyType.WestExt, specs.WestExt);
            SetModifyToolValue(ElementPropertyType.NorthExt, specs.NorthExt);
            SetModifyToolValue(ElementPropertyType.SouthExt, specs.SouthExt);
        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {
            
        }
    }
}
