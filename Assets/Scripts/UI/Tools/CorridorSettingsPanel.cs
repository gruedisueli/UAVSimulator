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
        public void Initialize(SimulationSettingsMainPanel mainPanel, DroneSettings droneSettings, float flightElevM, float sepDistM, bool isMetric)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            UpdateIsMetric(isMetric);
            _sepDistM = sepDistM;
            if (isMetric)
            {
                SetModifyToolValue(ElementPropertyType.Separation, _sepDistM);
            }
            else
            {
                SetModifyToolValue(ElementPropertyType.Separation, UnitUtils.MetersToFeet(_sepDistM));
            }
            InitializeDroneSettings(droneSettings, flightElevM, isMetric);
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);

            if (args.Update.ElementPropertyType == ElementPropertyType.Separation && args.Update is ModifyFloatPropertyArg f)
            {
                _sepDistM = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
            }
            _mainPanel.UpdateCorridorDroneSettings(_droneSettingsCopy, _flightElevM, _sepDistM);
        }
    }
}
