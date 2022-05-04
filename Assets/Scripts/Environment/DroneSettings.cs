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
        public EventHandler<System.EventArgs> OnModified;
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
        
        public FloatRange MaxSpeedRange_MPS { get; } = new FloatRange(1.0f, 250.0f);
        [JsonProperty] private float _maxSpeed_MPS;
        public float MaxSpeed_MPS
        {
            get => _maxSpeed_MPS;
            set => _maxSpeed_MPS = value;
        }

        public FloatRange TakeOffSpeedRange_MPS { get; } = new FloatRange(1.0f, 250.0f);
        [JsonProperty] private float _takeOffSpeed_MPS;
        public float TakeOffSpeed_MPS
        {
            get => _takeOffSpeed_MPS;
            set => _takeOffSpeed_MPS = value;
        }

        public FloatRange LandingOffSpeedRange_MPS { get; } = new FloatRange(1.0f, 250.0f);
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

        public FloatRange SoundAtSourceRange_Db { get; } = new FloatRange(50.0f, 150.0f);
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

        public DroneSettings(DroneSettings orig)
        {
            _droneType = orig.DroneType;
            _yawSpeed = orig.YawSpeed;
            _maxSpeed_MPS = orig.MaxSpeed_MPS;
            _takeOffSpeed_MPS = orig.TakeOffSpeed_MPS;
            _landingSpeed_MPS = orig.LandingSpeed_MPS;
            _waitTime_S = orig.WaitTime_S;
            _capacity = orig.Capacity;
            _soundAtSource_Decibels = orig.SoundAtSource_Decibels;
        }

        public void ApplySettings(DroneSettings s)
        {
            _droneType = s.DroneType;
            _yawSpeed = s.YawSpeed;
            _maxSpeed_MPS = s.MaxSpeed_MPS;
            _takeOffSpeed_MPS = s.TakeOffSpeed_MPS;
            _landingSpeed_MPS = s.LandingSpeed_MPS;
            _waitTime_S = s.WaitTime_S;
            _capacity = s.Capacity;
            _soundAtSource_Decibels = s.SoundAtSource_Decibels;
            OnModified?.Invoke(this, new System.EventArgs());
        }

    }
}
