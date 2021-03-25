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

            var specs = sCity.CitySpecs;

            _infoPanel.SetTextElement(ElementPropertyType.Name, specs.Name);
            _infoPanel.SetTextElement(ElementPropertyType.Population, specs.Population.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.Jobs, specs.Jobs.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.EastExt, specs.EastExt.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.WestExt, specs.WestExt.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.NorthExt, specs.NorthExt.ToString());
            _infoPanel.SetTextElement(ElementPropertyType.SouthExt, specs.SouthExt.ToString());

        }

        protected override void StartModify(object sender, System.EventArgs args)
        {
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

        protected override void ModifyTextValues(IModifyElementArgs args)
        {
            try
            {
                switch (args.Update.ElementPropertyType)
                {
                    case ElementPropertyType.Name:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.Name, (args.Update as ModifyStringPropertyArg).Value);
                            break;
                        }
                    case ElementPropertyType.Population:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.Population, (args.Update as ModifyIntPropertyArg).Value.ToString());
                            break;
                        }
                    case ElementPropertyType.Jobs:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.Jobs, (args.Update as ModifyIntPropertyArg).Value.ToString());
                            break;
                        }
                    case ElementPropertyType.EastExt:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.EastExt, (args.Update as ModifyIntPropertyArg).Value.ToString());
                            break;
                        }
                    case ElementPropertyType.WestExt:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.WestExt, (args.Update as ModifyIntPropertyArg).Value.ToString());
                            break;
                        }
                    case ElementPropertyType.NorthExt:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.NorthExt, (args.Update as ModifyIntPropertyArg).Value.ToString());
                            break;
                        }
                    case ElementPropertyType.SouthExt:
                        {
                            _infoPanel.SetTextElement(ElementPropertyType.SouthExt, (args.Update as ModifyIntPropertyArg).Value.ToString());
                            break;
                        }
                }

            }
            catch
            {
                Debug.LogError("Casting error in city modify panel update");
                return;
            }
        }
    }
}
