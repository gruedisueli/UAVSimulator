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

        public override void SetSelectedState(bool isSelected)
        {
            throw new NotImplementedException();
        }
    }
}
