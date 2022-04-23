using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Scripts.Environment
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DroneSettings
    {
        //ALL UNITS ARE IN METRIC! THE UNITY SIMULATION IS METRIC-BASED

        [JsonProperty] private string _droneType;
        public string DroneType
        {
            get => _droneType;
        }

        [JsonProperty] private float _yawSpeed;
        public float YawSpeed
        {
            get => _yawSpeed;
        }

        [JsonProperty] private float _maxSpeed_MPS;
        public float MaxSpeed_MPS
        {
            get => _maxSpeed_MPS;
            set => _maxSpeed_MPS = value;
        }

        [JsonProperty] private float _takeOffSpeed_MPS;
        public float TakeOffSpeed_MPS
        {
            get => _takeOffSpeed_MPS;
            set => _takeOffSpeed_MPS = value;
        }

        [JsonProperty] private float _landingSpeed_MPS;

        public float LandingSpeed_MPS
        {
            get => _landingSpeed_MPS;
            set => _landingSpeed_MPS = value;
        }

        [JsonProperty] private float _waitTime_S;

        public float WaitTime_S
        {
            get => _waitTime_S;
            set => _waitTime_S = value;
        }

        [JsonProperty] private int _capacity;

        public int Capacity
        {
            get => _capacity;
            set => _capacity = value;
        }

        [JsonProperty] private float _soundAtSource_Decibels;

        public float SoundAtSource_Decibels
        {
            get => _soundAtSource_Decibels;
            set => _soundAtSource_Decibels = value;
        }

        public DroneSettings(string droneType, float yawSpeed, float maxSpeedMPS, float takeOffSpeedMPS, float landingSpeedMPS, float waitTimeS, int capacity, float soundAtSourceDb)
        {
            _droneType = droneType;
            _yawSpeed = yawSpeed;
            _maxSpeed_MPS = maxSpeedMPS;
            _takeOffSpeed_MPS = takeOffSpeedMPS;
            _landingSpeed_MPS = landingSpeedMPS;
            _waitTime_S = waitTimeS;
            _capacity = capacity;
            _soundAtSource_Decibels = soundAtSourceDb;
        }

    }
}
