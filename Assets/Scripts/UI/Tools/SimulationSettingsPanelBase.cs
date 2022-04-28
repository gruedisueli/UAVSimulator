using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.EventArgs;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class SimulationSettingsPanelBase : MonoBehaviour
    {
        protected SimulationSettingsMainPanel _mainPanel;
        private ModifyTool[] _modifyTools;
        private bool _loadedModifyTools = false;

        void Awake()
        {
            
        }

        protected void GetModifyTools()
        {
            if (_loadedModifyTools) return;
            _modifyTools = GetComponentsInChildren<ModifyTool>(true);
            if (_modifyTools == null)
            {
                Debug.LogError("Could not find modify tools in children of  simulation settings panel");
                return;
            }

            foreach (var m in _modifyTools)
            {
                m.OnElementModified += OnModification;
            }

            _loadedModifyTools = true;
        }

        protected void UpdateIsMetric(bool isMetric)
        {
            foreach (var m in _modifyTools)
            {
                if (m is ModifyFieldTool f)
                {
                    f.UpdateUnits(isMetric);
                }
            }
        }

        protected virtual void OnModification(IModifyElementArgs args)
        {

        }

        /// <summary>
        /// Sets the value of a modify tool on this panel.
        /// </summary>
        protected void SetModifyToolValue(ElementPropertyType pT, int value)
        {
            foreach (var m in _modifyTools)
            {
                if (m._propertyType != pT) continue;
                if (m is SliderTool sT)
                {
                    sT.SetValue(value);
                }
                else if (m is ModifyFieldTool fT)
                {
                    fT.SetCurrentValue(value.ToString());
                }
                break;
            }
        }

        /// <summary>
        /// Sets the value of a modify tool on this panel.
        /// </summary>
        protected void SetModifyToolValue(ElementPropertyType pT, float value)
        {
            foreach (var m in _modifyTools)
            {
                if (m._propertyType != pT) continue;
                if (m is SliderTool sT)
                {
                    sT.SetValue(value);
                }
                else if (m is ModifyFieldTool fT)
                {
                    fT.SetCurrentValue(value.ToString("F2"));
                }
                break;
            }
        }

        /// <summary>
        /// Sets the value of a modify tool on this panel.
        /// </summary>
        protected void SetModifyToolValue(ElementPropertyType pT, String value)
        {
            foreach (var m in _modifyTools)
            {
                if (m._propertyType != pT) continue;
                else if (m is ModifyFieldTool fT)
                {
                    fT.SetCurrentValue(value);
                }
                break;
            }
        }
    }
}
