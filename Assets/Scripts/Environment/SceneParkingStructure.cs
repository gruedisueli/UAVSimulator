using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The instance of a parking structure in the scene itself.
    /// </summary>
    public class SceneParkingStructure : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public ParkingStructureBase ParkingStructureSpecs { get; private set; }
        public ParkingControl ParkingCtrl { get; private set; }

        public void Initialize(ParkingStructureBase pS, string guid)
        {
            Guid = guid;
            ParkingStructureSpecs = pS;
            gameObject.tag = "ParkingStructure";
            gameObject.name = "Parking_" + pS.Type;
            gameObject.layer = pS.Layer;
            pS.StandbyPosition = new Vector3(0, 400, 0);
            pS.LandingQueueHead = new Vector3(10, 400, 0);
            pS.LandingQueueDirection = new Vector3(1, 0, 0);
            ParkingCtrl = gameObject.AddComponent<ParkingControl>();
            ParkingCtrl.parkingInfo = pS;


            UpdateGameObject();
        }

        public override void UpdateGameObject()
        {
            gameObject.transform.position = ParkingStructureSpecs.Position;
            gameObject.transform.rotation = Quaternion.Euler(ParkingStructureSpecs.Rotation.x, ParkingStructureSpecs.Rotation.y, ParkingStructureSpecs.Rotation.z);
            gameObject.transform.localScale = ParkingStructureSpecs.Scale;
        }
    }
}
