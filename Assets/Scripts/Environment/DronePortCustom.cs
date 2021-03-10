using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class DronePortCustom : DronePortBase
    {
        [SerializeField]
        private string _type = "";
        public override string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
        private Vector3 _position = new Vector3();
        public override Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        [SerializeField]
        private Vector3 _rotation = new Vector3();
        public override Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        [SerializeField]
        private Vector3 _scale = new Vector3();
        public override Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        [SerializeField]
        private Vector3 _standByPosition = new Vector3();
        public override Vector3 StandbyPosition
        {
            get
            {
                return _standByPosition;
            }
            set
            {
                _standByPosition = value;
            }
        }

        [SerializeField]
        private Vector3 _landingQueueHead = new Vector3();
        public override Vector3 LandingQueueHead
        {
            get
            {
                return _landingQueueHead;
            }
            set
            {
                _landingQueueHead = value;
            }
        }

        [SerializeField]
        private Vector3 _landingQueueDirection = new Vector3();
        public override Vector3 LandingQueueDirection
        {
            get
            {
                return _landingQueueDirection;
            }
            set
            {
                _landingQueueDirection = value;
            }
        }

        [SerializeField]
        private Vector3 _landingPoint = new Vector3();
        public override Vector3 LandingPoint
        {
            get
            {
                return _landingPoint;
            }
            set
            {
                _landingPoint = value;
            }
        }

        [SerializeField]
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

        [SerializeField]
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

        [SerializeField]
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

        [SerializeField]
        private bool _isScalable = false;
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

        public override DronePortBase GetCopy()
        {
            return new DronePortCustom(this);
        }

        public DronePortCustom(DronePortCustom dP)
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
