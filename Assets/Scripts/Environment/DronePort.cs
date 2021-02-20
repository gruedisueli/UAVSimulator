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
        //public DronePortType PortType
        //{
        //    get
        //    {
        //        return _portType;
        //    }
        //    set
        //    {
        //        _portType = value;
        //    }
        //}
        //public Vector3 Position
        //{
        //    get
        //    {
        //        return _position != null ? new Vector3(_position[0], _position[1], _position[2]) : new Vector3();
        //    }
        //    set
        //    {
        //        _position = new float[] { value.x, value.y, value.z };
        //    }
        //}
        //public double Rotation
        //{
        //    get
        //    {
        //        return _rotation;
        //    }
        //    set
        //    {
        //        _rotation = value;
        //    }
        //}
        //public Vector3 Scale
        //{
        //    get
        //    {
        //        return _scale != null ? new Vector3(_scale[0], _scale[1], _scale[2]) : new Vector3();
        //    }
        //    set
        //    {
        //        _scale = new float[] { value.x, value.y, value.z };
        //    }
        //}
        //public Vector3 LandingQueueHead
        //{
        //    get
        //    {
        //        return _landingQueueHead != null ? new Vector3(_landingQueueHead[0], _landingQueueHead[1], _landingQueueHead[2]) : new Vector3();
        //    }
        //    set
        //    {
        //        _landingQueueHead = new float[] { value.x, value.y, value.z };
        //    }
        //}
        //public Vector3 LandingQueueDirection
        //{
        //    get
        //    {
        //        return _landingQueueDirection != null ? new Vector3(_landingQueueDirection[0], _landingQueueDirection[1], _landingQueueDirection[2]) : new Vector3();
        //    }
        //    set
        //    {
        //        _landingQueueDirection = new float[] { value.x, value.y, value.z };
        //    }
        //}

        //unity serialization is incredibly dumb and requires fields to be public
        //their "SerializeField" attribute does not work.
        public DronePortType _portType;
        public Vector3 _position;
        public double _rotation;
        public Vector3 _scale;
        public Vector3 _landingQueueHead;
        public Vector3 _landingQueueDirection;

        public DronePort()
        {

        }
    }
}
