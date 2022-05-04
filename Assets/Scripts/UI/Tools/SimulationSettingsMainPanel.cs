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
        private VehicleControlSystem _vehicleControlSystem;

        void Awake()
        {
            if (_generalSettings == null || _corridorSettings == null || _lowAltitudeSettings == null || _backgroundSettings == null)
            {
                Debug.LogError("Not all panels defined for simulation settings main panel");
                return;
            }

            _vehicleControlSystem = FindObjectOfType<VehicleControlSystem>(true);
            if (_vehicleControlSystem == null)
            {
                Debug.LogError("Could not find vehicle control system in scene");
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
            else
            {
                InitializeGeneral();
            }
        }

        public void UpdateCorridorDroneSettings(DroneSettings droneSettings, float flightElevM, float sepDistM)
        {
            _simulationSettingsCopy.CorridorDroneSettings = new DroneSettings(droneSettings);
            _simulationSettingsCopy.CorridorFlightElevation_M = flightElevM;
            _simulationSettingsCopy.CorridorSeparationDistance_M = sepDistM;
            InitializeCorridor();
        }

        public void UpdateLowAltitudeDroneSettings(DroneSettings droneSettings, float flightElevM, float travelRadiusM)
        {
            _simulationSettingsCopy.LowAltitudeDroneSettings = new DroneSettings(droneSettings);
            _simulationSettingsCopy.LowAltitudeFlightElevation_M = flightElevM;
            _simulationSettingsCopy.LowAltitudeDroneTravelRadius_M = travelRadiusM;
            InitializeLowAlt();
        }

        public void UpdateBackgroundDroneSettings(int droneCt, float upperElevM, float lowerElevM)
        {
            _simulationSettingsCopy.BackgroundDroneCount = droneCt;
            _simulationSettingsCopy.BackgroundDroneUpperElev_M = upperElevM;
            _simulationSettingsCopy.BackgoundDroneLowerElev_M = lowerElevM;
            InitializeBackground();
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
            bool rebuildNetwork = Math.Abs(_simulationSettingsCopy.CorridorFlightElevation_M - EnvironManager.Instance.Environ.SimSettings.CorridorFlightElevation_M) > 0.1 || Math.Abs(_simulationSettingsCopy.CorridorSeparationDistance_M - EnvironManager.Instance.Environ.SimSettings.CorridorSeparationDistance_M) > 0.1;
            var updateBackgroundDrones = _simulationSettingsCopy.BackgroundDroneCount != EnvironManager.Instance.Environ.SimSettings.BackgroundDroneCount;

            EnvironManager.Instance.Environ.SimSettings.ApplySettings(_simulationSettingsCopy);

            if (rebuildNetwork)
            {
                _vehicleControlSystem.RebuildNetwork();
            }
            if (updateBackgroundDrones)
            {
                _vehicleControlSystem.UpdateVehicleCount(_simulationSettingsCopy.BackgroundDroneCount);
            }
            gameObject.SetActive(false);
        }

        public void ResetToDefaults()
        {
            _simulationSettingsCopy = new SimulationSettings();
            InitializeAllPanels();
        }
        private void InitializeAllPanels()
        {
            InitializeGeneral();
            InitializeCorridor();
            InitializeLowAlt();
            InitializeBackground();
        }

        private void InitializeGeneral()
        {
            var s = _simulationSettingsCopy;
            _generalSettings.Initialize(this, s.IsMetricUnits, s.CallGenerationInterval_S, s.AcceptableNoiseThreshold_Decibels, s.SimulationSpeedMultiplier, s.CallGenIntervalRange_S, s.AcceptableNoiseThresholdRange_Db, s.SimulationSpeedMultiplierRange);
        }

        private void InitializeCorridor()
        {
            var s = _simulationSettingsCopy;
            _corridorSettings.Initialize(this, s.CorridorDroneSettings, s.CorridorFlightElevation_M, s.CorridorSeparationDistance_M, s.CorridorFlightElevRange_M, s.CorridorSeparationDistanceRange_M, s.IsMetricUnits);
        }

        private void InitializeLowAlt()
        {
            var s = _simulationSettingsCopy;
            _lowAltitudeSettings.Initialize(this, s.LowAltitudeDroneSettings, s.LowAltitudeFlightElevation_M, s.LowAltitudeDroneTravelRadius_M, s.LowAltFlightElevationRange_M, s.LowAltTravelRadiusRange_M, s.IsMetricUnits);
        }

        private void InitializeBackground()
        {
            var s = _simulationSettingsCopy;
            _backgroundSettings.Initialize(this, s.BackgroundDroneCount, s.BackgroundDroneUpperElev_M, s.BackgoundDroneLowerElev_M, s.BackgroundDroneUpperElevRange_M, s.BackgroundDroneLowerElevRange_M, s.BackgroundDroneCountRange, s.IsMetricUnits);
        }

    }
}
