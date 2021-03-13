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
    public delegate void CloseCityPanel();

    /// <summary>
    /// A panel that contains tools for modifying a city. Doesn't modify the city itself. There are child objects for this.
    /// </summary>
    public class CityPanel : MonoBehaviour
    {
        public Text _cityName;
        public Text _population;
        public Text _jobs;
        public Text _eastExt;
        public Text _westExt;
        public Text _northExt;
        public Text _southExt;

        public event CloseCityPanel OnCloseCityPanel;

        private ModifyPanel _modifyPanel;
        private ModifyTool[] _childTools;

        private void Start()
        {
            _childTools = GetComponentsInChildren<ModifyTool>(true);
            _modifyPanel = GetComponentInChildren<ModifyPanel>(true);
            if (_modifyPanel == null)
            {
                Debug.LogError("Modify panel not found in city panel");
                return;
            }

            foreach (var t in _childTools)
            {
                t.OnElementModified += Modification;
            }

            _modifyPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (var t in _childTools)
            {
                t.OnElementModified -= Modification;
            }
        }

        public void SetCity(CityOptions stats)
        {
            _cityName.text = "Name: " + stats.Name;
            _population.text = "Pop: " + stats.Population.ToString();
            _jobs.text = "Jobs: " + stats.Jobs.ToString();
            _eastExt.text = "East Ext: " + stats.EastExt.ToString();
            _westExt.text = "West Ext: " + stats.WestExt.ToString();
            _northExt.text = "North Ext: " + stats.NorthExt.ToString();
            _southExt.text = "South Ext: " + stats.SouthExt.ToString();
        }

        /// <summary>
        /// Called to activate/deactivate this entire game object and children.
        /// </summary>
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        /// <summary>
        /// Called when closing this panel.
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            OnCloseCityPanel.Invoke();
        }

        /// <summary>
        /// We need to subscribe to events from child objects so we can update text on this panel.
        /// </summary>
        private void Modification(IModifyElementArgs args)
        {
            try
            {
                switch (args.Update.Type)
                {
                    case ElementPropertyType.Name:
                        {
                            _cityName.text = "Name: " + (args.Update as ModifyStringPropertyArg).Value;
                            break;
                        }
                    case ElementPropertyType.Population:
                        {
                            _population.text = "Pop: " + (args.Update as ModifyIntPropertyArg).Value.ToString();
                            break;
                        }
                    case ElementPropertyType.Jobs:
                        {
                            _jobs.text = "Jobs: " + (args.Update as ModifyIntPropertyArg).Value.ToString();
                            break;
                        }
                    case ElementPropertyType.EastExt:
                        {
                            _eastExt.text = "East Ext: " + (args.Update as ModifyIntPropertyArg).Value.ToString();
                            break;
                        }
                    case ElementPropertyType.WestExt:
                        {
                            _westExt.text = "West Ext: " + (args.Update as ModifyIntPropertyArg).Value.ToString();
                            break;
                        }
                    case ElementPropertyType.NorthExt:
                        {
                            _northExt.text = "North Ext: " + (args.Update as ModifyIntPropertyArg).Value.ToString();
                            break;
                        }
                    case ElementPropertyType.SouthExt:
                        {
                            _southExt.text = "South Ext: " + (args.Update as ModifyIntPropertyArg).Value.ToString();
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
