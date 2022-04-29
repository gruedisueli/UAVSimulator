using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.EventArgs;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class GeneralSimulationSettingsPanel : SimulationSettingsPanelBase
    {
        public Slider _unitSelector;
        private bool _isMetric;
        private float _callGenInt, _acceptNoiseThresh, _simSpeedMult;
        private FloatRange _callGenRange, _noiseRange, _simSpeedRange;
        public void Initialize(SimulationSettingsMainPanel mainPanel, bool isMetric, float callGenInt, float acceptNoiseThresh, float simSpeedMult, FloatRange callGenRange, FloatRange noiseRange, FloatRange simSpeedRange)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            if (_unitSelector == null)
            {
                Debug.LogError("Unit selection dropdown not specified for general simulation settings panel");
                return;
            }
            _isMetric = isMetric;
            _callGenInt = callGenInt;
            _acceptNoiseThresh = acceptNoiseThresh;
            _simSpeedMult = simSpeedMult;
            _callGenRange = callGenRange;
            _noiseRange = noiseRange;
            _simSpeedRange = simSpeedRange;
            _unitSelector.SetValueWithoutNotify(isMetric ? 0 : 1);
            SetModifyToolValue(ElementPropertyType.CallGenInterval, callGenInt);
            SetModifyToolValue(ElementPropertyType.AcceptableNoiseThreshold, acceptNoiseThresh);
            SetModifyToolValue(ElementPropertyType.SimulationSpeedMultiplier, simSpeedMult);
        }

        public void UnitChange()
        {
            _isMetric = _unitSelector.value == 0;
            _mainPanel.UpdateGeneralSettings(_isMetric, _callGenInt, _acceptNoiseThresh, _simSpeedMult);
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);

            switch (args.Update.ElementPropertyType)
            {
                case ElementPropertyType.CallGenInterval:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        _callGenInt = _callGenRange.ClampToRange(f.Value);
                    }
                    break;
                }
                case ElementPropertyType.AcceptableNoiseThreshold:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        _acceptNoiseThresh = _noiseRange.ClampToRange(f.Value);
                    }
                    break;
                }
                case ElementPropertyType.SimulationSpeedMultiplier:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        _simSpeedMult = _simSpeedRange.ClampToRange(f.Value);
                    }
                    break;
                }
            }

            _mainPanel.UpdateGeneralSettings(_isMetric, _callGenInt, _acceptNoiseThresh, _simSpeedMult);
        }
    }
}
