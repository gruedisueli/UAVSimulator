using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.Tags;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The instance of a parking structure in the scene itself.
    /// </summary>
    public class SceneParkingStructure : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override bool Is2D { get; protected set; } = false;
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public override bool IsSelectable { get; protected set; } = false;
        public ParkingStructureBase ParkingStructureSpecs { get; private set; }
        public ParkingControl ParkingCtrl { get; private set; }

        public void Initialize(ParkingStructureBase pS, string guid, bool is2d, Canvas canvas, bool isSelectable)
        {
            Guid = guid;
            Is2D = is2d;
            SceneCanvas = canvas;
            if (Is2D) Sprite2d = Instantiate(EnvironManager.Instance.ParkingSpritePrefab, SceneCanvas.transform);
            IsSelectable = isSelectable;
            if (IsSelectable) MakeSelectable();
            ParkingStructureSpecs = pS;
            gameObject.tag = "ParkingStructure";
            gameObject.name = "Parking_" + pS.Type;
            gameObject.layer = pS.Layer;
            pS.StandbyPosition = new Vector3(0, 150, 0);
            pS.LandingQueueHead = new Vector3(10, 150, 0);
            pS.LandingQueueDirection = new Vector3(1, 0, 0);
            ParkingCtrl = gameObject.AddComponent<ParkingControl>();
            ParkingCtrl.parkingInfo = pS;


            UpdateGameObject();
        }

        public override void UpdateGameObject()
        {
            if (Is2D)
            {
                var tag = Sprite2d.GetComponent<UIWorldTag>();
                tag?.SetWorldPos(ParkingStructureSpecs.Position);
            }

            gameObject.transform.position = ParkingStructureSpecs.Position;
            gameObject.transform.rotation = Quaternion.Euler(ParkingStructureSpecs.Rotation.x, ParkingStructureSpecs.Rotation.y, ParkingStructureSpecs.Rotation.z);
            gameObject.transform.localScale = ParkingStructureSpecs.Scale;
           
        }
    }
}
