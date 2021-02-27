using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class DronePort
    {
        [SerializeField]
        private string _type = "";
        public string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
        private Vector3 _position = new Vector3();
        public Vector3 Position
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
        public Vector3 Rotation
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
        public Vector3 Scale
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
        public Vector3 StandbyPosition
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
        public Vector3 LandingQueueHead
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
        public Vector3 LandingQueueDirection
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
        public Vector3 LandingPoint
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
        public float MaximumVehicleSize
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
        public bool IsMountable
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
        public bool IsOnTheGround
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
        public bool IsScalable
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

        public DronePort()
        {

        }

        public DronePort(DronePort dP)
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

        // Translates to the global coordinate
        public Vector3 TranslateLandingGuidePosition(Vector3 parkingSpot)
        {
            return (Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z) * parkingSpot + Position);
        }

        public List<Vector3> GetLandingGuide(string mode)
        {
            // mode == {parking, unparking}
            // Temporary function

            List<Vector3> guides = new List<Vector3>();

            guides.Add(LandingQueueHead);
            guides.Add(StandbyPosition);
            guides.Add(LandingPoint);

            /*
            if (type.Equals("Simple_4Way_Stack"))
            {
                Vector3 direction = new Vector3(spot.x, 0.0f, spot.z).normalized;
                direction = Quaternion.Euler(rotation.x, rotation.y, rotation.z) * direction;
                guides.Add(spot);
                Vector3 current_spot = spot + direction * 20.0f;
                guides.Add(current_spot);
                current_spot.y = standbyPosition.y;
                guides.Add(current_spot);
                guides.Add(standbyPosition);
            }
            else if (type.Equals("generic_rectangular_lot"))
            {
                guides.Add(spot);
                guides.Add(standbyPosition);
            }*/


            if (mode == "takeoff")
            {
                guides.Reverse();
            }

            return guides;
        }
    }
}
