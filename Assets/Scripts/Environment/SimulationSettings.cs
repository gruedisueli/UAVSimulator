using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Scripts.Environment
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SimulationSettings
    {
        //ALL UNITS ARE IN METRIC! THE UNITY SIMULATION IS METRIC-BASED

        #region GENERAL SETTINGS

        [JsonProperty] 
        private bool _isMetricUnits = false;
        public bool IsMetricUnits
        {
            get => _isMetricUnits;
            set => _isMetricUnits = value;
        }

        [JsonProperty] private float _callGenerationInterval_S = 2.0f;

        public float CallGenerationInterval_S
        {
            get => _callGenerationInterval_S;
            set => _callGenerationInterval_S = value;
        }

        [JsonProperty] private float _acceptableNoiseThreshold_Decibels = 50.0f;

        public float AcceptableNoiseThreshold_Decibels
        {
            get => _acceptableNoiseThreshold_Decibels;
            set => _acceptableNoiseThreshold_Decibels = value;
        }

        [JsonProperty] private float _simulationSpeedMultiplier = 1.0f;

        public float SimulationSpeedMultiplier
        {
            get => _simulationSpeedMultiplier;
            set => _simulationSpeedMultiplier = value;
        }

        [JsonProperty]
        private string _strategicDeconfliction = "none";

        public string StrategicDeconfliction
        {
            get => _strategicDeconfliction;
            set => _strategicDeconfliction = value;
        }

        #endregion

        #region CORRIDOR DRONE SETTINGS

        [JsonProperty] private DroneSettings _corridorDroneSettings = new DroneSettings("corridor", 30.0f, 50.0f, 10.0f, 10.0f, 0.0f, 5, 125.0f);

        public DroneSettings CorridorDroneSettings
        {
            get => _corridorDroneSettings;
            set => _corridorDroneSettings = value;
        }


        [JsonProperty] private float _corridorFlightElevation_M = 152.4f;

        public float CorridorFlightElevation_M
        {
            get => _corridorFlightElevation_M;
            set => _corridorFlightElevation_M = value;
        }

        [JsonProperty] private float _corridorSeparationDistance_M = 25.0f;

        public float CorridorSeparationDistance_M
        {
            get => _corridorSeparationDistance_M;
            set => _corridorSeparationDistance_M = value;
        }

        #endregion

        #region LOW ALTITUDE DRONE SETTINGS

        [JsonProperty] private DroneSettings _lowAltitudeDroneSettings = new DroneSettings("LowAltitude", 50.0f, 40.0f, 25.0f, 25.0f, 0.0f, 0, 125.0f);

        public DroneSettings LowAltitudeDroneSettings
        {
            get => _lowAltitudeDroneSettings;
            set => _lowAltitudeDroneSettings = value;
        }

        [JsonProperty] private float _lowAltitudeFlightElevation_M = 152.4f;

        public float LowAltitudeFlightElevation_M
        {
            get => _lowAltitudeFlightElevation_M;
            set => _lowAltitudeFlightElevation_M = value;
        }

        [JsonProperty] private float _lowAltitudeDroneTravelRadius_M = 5000.0f;

        public float LowAltitudeDroneTravelRadius_M
        {
            get => _lowAltitudeDroneTravelRadius_M;
            set => _lowAltitudeDroneTravelRadius_M = value;
        }

        #endregion

        #region BACKGROUND DRONE SETTINGS
        
        //note that "background" drones are instantiations of one of the explicitly defined types above; other properties previously defined.

        [JsonProperty] private int _backgroundDroneCount = 250;

        public int BackgroundDroneCount
        {
            get => _backgroundDroneCount;
            set => _backgroundDroneCount = value;
        }

        [JsonProperty] private float _backgroundDroneUpperElev_M = 135;

        public float BackgroundDroneUpperElev_M
        {
            get => _backgroundDroneUpperElev_M;
            set => _backgroundDroneUpperElev_M = value;
        }

        [JsonProperty] private float _backgroundDroneLowerElev_M = 100;

        public float BackgoundDroneLowerElev_M
        {
            get => _backgroundDroneLowerElev_M;
            set => _backgroundDroneLowerElev_M = value;
        }

        #endregion

        public SimulationSettings()
        {

        }

        public SimulationSettings (SimulationSettings orig)
        {
            _isMetricUnits = orig.IsMetricUnits;
            _callGenerationInterval_S = orig.CallGenerationInterval_S;
            _acceptableNoiseThreshold_Decibels = orig.AcceptableNoiseThreshold_Decibels;
            _simulationSpeedMultiplier = orig.SimulationSpeedMultiplier;
            _strategicDeconfliction = orig.StrategicDeconfliction;
            _corridorDroneSettings = new DroneSettings(orig.CorridorDroneSettings);
            _corridorFlightElevation_M = orig.CorridorFlightElevation_M;
            _corridorSeparationDistance_M = orig.CorridorSeparationDistance_M;
            _lowAltitudeDroneSettings = new DroneSettings(orig.LowAltitudeDroneSettings);
            _lowAltitudeFlightElevation_M = orig.LowAltitudeFlightElevation_M;
            _lowAltitudeDroneTravelRadius_M = orig.LowAltitudeDroneTravelRadius_M;
            _backgroundDroneCount = orig.BackgroundDroneCount;
            _backgroundDroneUpperElev_M = orig.BackgroundDroneUpperElev_M;
            _backgroundDroneLowerElev_M = orig.BackgoundDroneLowerElev_M;
        }
    }
}
