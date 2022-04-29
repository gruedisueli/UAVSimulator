using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class SimulationSettingsMainPanel : MonoBehaviour
    {
        public GeneralSimulationSettingsPanel _generalSettings;
        public CorridorSettingsPanel _corridorSettings;
        public LowAltitudeSettingsPanel _lowAltitudeSettings;
        public BackgroundSettingsPanel _backgroundSettings;

        private SimulationSettings _simulationSettingsCopy;

        void Awake()
        {
            if (_generalSettings == null || _corridorSettings == null || _lowAltitudeSettings == null || _backgroundSettings == null)
            {
                Debug.LogError("Not all panels defined for simulation settings main panel");
                return;
            }
        }
        public void UpdateGeneralSettings(bool isMetric, float callGenInt, float acceptNoiseThresh, float simSpeedMult)
        {
            _simulationSettingsCopy.CallGenerationInterval_S = callGenInt;
            _simulationSettingsCopy.AcceptableNoiseThreshold_Decibels = acceptNoiseThresh;
            _simulationSettingsCopy.SimulationSpeedMultiplier = simSpeedMult;
            if (isMetric != _simulationSettingsCopy.IsMetricUnits)
            {
                _simulationSettingsCopy.IsMetricUnits = isMetric;
                InitializeAllPanels();//need to refresh units and numbers
            }
        }

        public void UpdateCorridorDroneSettings(DroneSettings droneSettings, float flightElevM, float sepDistM)
        {
            _simulationSettingsCopy.CorridorDroneSettings = new DroneSettings(droneSettings);
            _simulationSettingsCopy.CorridorFlightElevation_M = flightElevM;
            _simulationSettingsCopy.CorridorSeparationDistance_M = sepDistM;
        }

        public void UpdateLowAltitudeDroneSettings(DroneSettings droneSettings, float flightElevM)
        {
            _simulationSettingsCopy.LowAltitudeDroneSettings = new DroneSettings(droneSettings);
            _simulationSettingsCopy.LowAltitudeFlightElevation_M = flightElevM;
        }

        public void UpdateBackgroundDroneSettings(int droneCt, float upperElevM, float lowerElevM)
        {
            _simulationSettingsCopy.BackgroundDroneCount = droneCt;
            _simulationSettingsCopy.BackgroundDroneUpperElev_M = upperElevM;
            _simulationSettingsCopy.BackgoundDroneLowerElev_M = lowerElevM;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            _simulationSettingsCopy = new SimulationSettings(EnvironManager.Instance.Environ.SimSettings);
            InitializeAllPanels();
        }

        public void Cancel()
        {
            gameObject.SetActive(false);
        }

        public void Ok()
        {
            EnvironManager.Instance.Environ.SimSettings = new SimulationSettings(_simulationSettingsCopy);
            gameObject.SetActive(false);
        }

        public void ResetToDefaults()
        {
            _simulationSettingsCopy = new SimulationSettings();
            InitializeAllPanels();
        }
        private void InitializeAllPanels()
        {
            var s = _simulationSettingsCopy;
            _generalSettings.Initialize(this, s.IsMetricUnits, s.CallGenerationInterval_S, s.AcceptableNoiseThreshold_Decibels, s.SimulationSpeedMultiplier);
            _corridorSettings.Initialize(this, s.CorridorDroneSettings, s.CorridorFlightElevation_M, s.CorridorSeparationDistance_M, s.IsMetricUnits);
            _lowAltitudeSettings.Initialize(this, s.LowAltitudeDroneSettings, s.LowAltitudeFlightElevation_M, s.IsMetricUnits);
            _backgroundSettings.Initialize(this, s.BackgroundDroneCount, s.BackgroundDroneUpperElev_M, s.BackgoundDroneLowerElev_M, s.IsMetricUnits);
        }

    }
}
