using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class DronePortRect : DronePortBase
    {
        [JsonProperty]
        private string _type = "Rect";
        public override string Type
        {
            get
            {
                return _type;
            }
        }

        [JsonProperty]
        private SerVect3f _position = new SerVect3f();
        public override Vector3 Position
        {
            get
            {
                return _position.ToVector3();
            }
            set
            {
                _position = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _rotation = new SerVect3f();
        public override Vector3 Rotation
        {
            get
            {
                return _rotation.ToVector3();
            }
            set
            {
                _rotation = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _scale = new SerVect3f(1, 1, 1);
        public override Vector3 Scale
        {
            get
            {
                return _scale.ToVector3();
            }
            set
            {
                _scale = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _standByPosition = new SerVect3f();
        public override Vector3 StandbyPosition
        {
            get
            {
                return _standByPosition.ToVector3();
            }
            set
            {
                _standByPosition = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _landingQueueHead = new SerVect3f();
        public override Vector3 LandingQueueHead
        {
            get
            {
                return _landingQueueHead.ToVector3();
            }
            set
            {
                _landingQueueHead = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _landingQueueDirection = new SerVect3f();
        public override Vector3 LandingQueueDirection
        {
            get
            {
                return _landingQueueDirection.ToVector3();
            }
            set
            {
                _landingQueueDirection = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private SerVect3f _landingPoint = new SerVect3f();
        public override Vector3 LandingPoint
        {
            get
            {
                return _landingPoint.ToVector3();
            }
            set
            {
                _landingPoint = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private float _maximumVehicleSize = 0;
        public override float MaximumVehicleSize
        {
            get
            {
                return _maximumVehicleSize;
            }
            set
            {
                _maximumVehicleSize = value;
            }
        }

        [JsonProperty]
        private bool _isMountable = false;
        public override bool IsMountable
        {
            get
            {
                return _isMountable;
            }
            set
            {
                _isMountable = value;
            }
        }

        [JsonProperty]
        private bool _isOnTheGround = false;
        public override bool IsOnTheGround
        {
            get
            {
                return _isOnTheGround;
            }
            set
            {
                _isOnTheGround = value;
            }
        }

        [JsonProperty]
        private bool _isScalable = true;
        public override bool IsScalable
        {
            get
            {
                return _isScalable;
            }
            set
            {
                _isScalable = value;
            }
        }

        public DronePortRect(Vector3 pos)
        {
            Position = pos;
        }

        public override DronePortBase GetCopy()
        {
            return new DronePortRect(this);
        }

        public DronePortRect(DronePortRect dP)
        {
            _type = dP.Type;
            Position = dP.Position;
            Rotation = dP.Rotation;
            Scale = dP.Scale;
            StandbyPosition = dP.StandbyPosition;
            LandingQueueHead = dP.LandingQueueHead;
            LandingQueueDirection = dP.LandingQueueDirection;
            LandingPoint = dP.LandingPoint;
            MaximumVehicleSize = dP.MaximumVehicleSize;
            IsMountable = dP.IsMountable;
            IsOnTheGround = dP.IsOnTheGround;
            IsScalable = dP.IsScalable;
        }
    }
}
