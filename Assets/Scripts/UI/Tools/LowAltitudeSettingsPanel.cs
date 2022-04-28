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
    public class LowAltitudeSettingsPanel : DroneSettingsPanel
    {
        public void Initialize(SimulationSettingsMainPanel mainPanel, DroneSettings droneSettings, float flightElevM, bool isMetric)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            UpdateIsMetric(isMetric);
            InitializeDroneSettings(droneSettings, flightElevM, isMetric);
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);

            _mainPanel.UpdateLowAltitudeDroneSettings(_droneSettingsCopy, _flightElevM);
        }
    }
}
