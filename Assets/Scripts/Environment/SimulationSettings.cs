using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;

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

        [JsonProperty] private float _corridorMaxSpeed_MPS = 50.0f;

        public float CorridorMaxSpeed_MPS
        {
            get => _corridorMaxSpeed_MPS;
            set => _corridorMaxSpeed_MPS = value;
        }

        [JsonProperty] private float _corridorTakeOffSpeed_MPS = 10.0f;
        public float CorridorTakeOffSpeed_MPS
        {
            get => _corridorTakeOffSpeed_MPS;
            set => _corridorTakeOffSpeed_MPS = value;
        }

        [JsonProperty] private float _corridorLandingSpeed_MPS = 10.0f;

        public float CorridorLandingSpeed_MPS
        {
            get => _corridorLandingSpeed_MPS;
            set => _corridorLandingSpeed_MPS = value;
        }

        [JsonProperty] private float _corridorWaitTime_S = 0;

        public float CorridorWaitTime_S
        {
            get => _corridorWaitTime_S;
            set => _corridorWaitTime_S = value;
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

        [JsonProperty] private int _corridorDroneCapacity = 5;

        public int CorridorDroneCapacity
        {
            get => _corridorDroneCapacity;
            set => _corridorDroneCapacity = value;
        }

        [JsonProperty] private float _corridorDroneSoundAtSource_Decibels = 80;

        public float CorridorDroneSoundAtSource_Decibels
        {
            get => _corridorDroneSoundAtSource_Decibels;
            set => _corridorDroneSoundAtSource_Decibels = value;
        }

        #endregion

        #region LOW ALTITUDE DRONE SETTINGS

        [JsonProperty] private float _lowAltitudeMaxSpeed_MPS = 40.0f;

        public float LowAltitudeMaxSpeed_MPS
        {
            get => _lowAltitudeMaxSpeed_MPS;
            set => _lowAltitudeMaxSpeed_MPS = value;
        }

        [JsonProperty] private float _lowAltitudeTakeOffSpeed_MPS = 25.0f;

        public float LowAltitudeTakeOffSpeed_MPS
        {
            get => _lowAltitudeTakeOffSpeed_MPS;
            set => _lowAltitudeTakeOffSpeed_MPS = value;
        }

        [JsonProperty] private float _lowAltitudeLandingSpeed_MPS = 25.0f;

        public float LowAltitudeLandingSpeed_MPS
        {
            get => _lowAltitudeLandingSpeed_MPS;
            set => _lowAltitudeLandingSpeed_MPS = value;
        }

        [JsonProperty] private float _lowAltitudeWaitTime_S = 0;

        public float LowAltitudeWaitTime_S
        {
            get => _lowAltitudeWaitTime_S;
            set => _lowAltitudeWaitTime_S = value;
        }

        [JsonProperty] private float _lowAltitudeFlightElevation_M = 150;

        public float LowAltitudeFlightElevation_M
        {
            get => _lowAltitudeFlightElevation_M;
            set => _lowAltitudeFlightElevation_M = value;
        }

        [JsonProperty] private int _lowAltitudeDroneCapacity = 0;

        public int LowAltitudeDroneCapacity
        {
            get => _lowAltitudeDroneCapacity;
            set => _lowAltitudeDroneCapacity = value;
        }

        [JsonProperty] private float _lowAltitudeDroneSoundAtSource_Decibels = 70;

        public float LowAltitudeDroneSoundAtSource_Decibels
        {
            get => _lowAltitudeDroneSoundAtSource_Decibels;
            set => _lowAltitudeDroneSoundAtSource_Decibels = value;
        }

        #endregion

        #region BACKGROUND DRONE SETTINGS

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
    }
}
