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
        private float _travelRadiusM;
        public void Initialize(SimulationSettingsMainPanel mainPanel, DroneSettings droneSettings, float flightElevM, float travelRadiusM, bool isMetric)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            UpdateIsMetric(isMetric);
            _travelRadiusM = travelRadiusM;
            if (isMetric)
            {
                SetModifyToolValue(ElementPropertyType.LowAltDroneTravelRadius, _travelRadiusM);
            }
            else
            {
                SetModifyToolValue(ElementPropertyType.LowAltDroneTravelRadius, UnitUtils.MetersToFeet(_travelRadiusM));
            }
            InitializeDroneSettings(droneSettings, flightElevM, isMetric);
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);

            if (args.Update.ElementPropertyType == ElementPropertyType.LowAltDroneTravelRadius && args.Update is ModifyFloatPropertyArg f)
            {
                _travelRadiusM = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
            }

            _mainPanel.UpdateLowAltitudeDroneSettings(_droneSettingsCopy, _flightElevM, _travelRadiusM);
        }
    }
}
