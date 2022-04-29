using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.Tags;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The instance of a parking structure in the scene itself.
    /// </summary>
    public class SceneParkingStructure : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public ParkingStructureBase ParkingStructureSpecs { get; private set; }
        public ParkingControl ParkingCtrl { get; private set; }

        public void Initialize(ParkingStructureBase pS, string guid, Canvas canvas)
        {
            Guid = guid;
            SceneCanvas = canvas;
            Sprite2d = Instantiate(EnvironManager.Instance.ParkingSpritePrefab, SceneCanvas.transform);
            var b = Sprite2d.GetComponentInChildren<Button>();
            if (b == null)
            {
                Debug.LogError("Button not found on parking structure sprite prefab");
            }
            b.onClick.AddListener(SpriteClicked);
            Sprite2d.transform.SetAsFirstSibling();
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
            MakeSelectable();
        }

        public override void UpdateGameObject()
        {
            var tag = Sprite2d.GetComponent<UIWorldTag>();
            tag?.SetWorldPos(ParkingStructureSpecs.Position);

            gameObject.transform.position = ParkingStructureSpecs.Position;
            gameObject.transform.rotation = Quaternion.Euler(ParkingStructureSpecs.Rotation.x, ParkingStructureSpecs.Rotation.y, ParkingStructureSpecs.Rotation.z);
            gameObject.transform.localScale = ParkingStructureSpecs.Scale;
           
        }
    }
}
