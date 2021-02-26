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
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 standbyPosition;
        public Vector3 landingQueueHead;
        public Vector3 landingQueueDirection;
        public Vector3 landingPoint;
        public float maximumVehicleSize;
        public bool isMountable;
        public bool isOnTheGround;
        public bool isScalable;

        public DronePort()
        {

        }

        public DronePort(DronePort dP)
        {
            type = dP.type;
            position = dP.position;
            rotation = dP.rotation;
            scale = dP.scale;
            standbyPosition = dP.standbyPosition;
            landingQueueHead = dP.landingQueueHead;
            landingQueueDirection = dP.landingQueueDirection;
            landingPoint = dP.landingPoint;
            maximumVehicleSize = dP.maximumVehicleSize;
            isMountable = dP.isMountable;
            isOnTheGround = dP.isOnTheGround;
            isScalable = dP.isScalable;
        }

        // Translates to the global coordinate
        public Vector3 TranslateLandingGuidePosition(Vector3 parkingSpot)
        {
            return (Quaternion.Euler(rotation.x, rotation.y, rotation.z) * parkingSpot + position);
        }

        public List<Vector3> GetLandingGuide(string mode)
        {
            // mode == {parking, unparking}
            // Temporary function

            List<Vector3> guides = new List<Vector3>();

            guides.Add(landingQueueHead);
            guides.Add(standbyPosition);
            guides.Add(landingPoint);

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
