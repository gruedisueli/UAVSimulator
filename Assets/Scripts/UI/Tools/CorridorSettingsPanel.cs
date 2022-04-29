using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class CorridorSettingsPanel : DroneSettingsPanel
    {
        private float _sepDistM;
        private FloatRange _sepDistRange;
        public void Initialize(SimulationSettingsMainPanel mainPanel, DroneSettings droneSettings, float flightElevM, float sepDistM, FloatRange flightElevRange, FloatRange sepDistRange, bool isMetric)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            UpdateIsMetric(isMetric);
            _sepDistM = sepDistM;
            _sepDistRange = sepDistRange;
            if (isMetric)
            {
                SetModifyToolValue(ElementPropertyType.Separation, _sepDistM);
            }
            else
            {
                SetModifyToolValue(ElementPropertyType.Separation, UnitUtils.MetersToFeet(_sepDistM));
            }
            InitializeDroneSettings(droneSettings, flightElevM, flightElevRange, isMetric);
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);

            if (args.Update.ElementPropertyType == ElementPropertyType.Separation && args.Update is ModifyFloatPropertyArg f)
            {
                var d = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
                _sepDistM = _sepDistRange.ClampToRange(d);
            }
            _mainPanel.UpdateCorridorDroneSettings(_droneSettingsCopy, _flightElevM, _sepDistM);
        }
    }
}
