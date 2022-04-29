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
    /// The instance of a drone port in the scene itself.
    /// </summary>
    public class SceneDronePort : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public DronePortBase DronePortSpecs { get; private set; }
        public DronePortControl DronePortCtrl { get; private set; }

        public void Initialize(DronePortBase dP, string guid, Canvas canvas)
        {
            Guid = guid;
            SceneCanvas = canvas;
            Sprite2d = Instantiate(EnvironManager.Instance.PortSpritePrefab, SceneCanvas.transform);
            var b = Sprite2d.GetComponentInChildren<Button>();
            if (b == null)
            {
                Debug.LogError("Button not found on drone port sprite prefab");
            }
            Sprite2d.transform.SetAsFirstSibling();
            b.onClick.AddListener(SpriteClicked);
            DronePortSpecs = dP;
            gameObject.name = "DronePort_" + dP.Type;
            gameObject.tag = "DronePort";
            gameObject.layer = dP.Layer;
            dP.StandbyPosition = new Vector3(0, 150, 0);
            dP.LandingQueueHead = new Vector3(10, 150, 0);
            dP.LandingQueueDirection = new Vector3(1, 0, 0);
            DronePortControl control = gameObject.AddComponent<DronePortControl>();
            control.dronePortInfo = dP;
            DronePortCtrl = control;
            UpdateGameObject();
            MakeSelectable();
        }

        /// <summary>
        /// Synchronizes the game object with latest info in environment drone port info.
        /// </summary>
        public override void UpdateGameObject()
        {
            var tag = Sprite2d.GetComponent<UIWorldTag>();
            tag?.SetWorldPos(DronePortSpecs.Position);
            
            gameObject.transform.position = DronePortSpecs.Position;
            gameObject.transform.rotation = Quaternion.Euler(DronePortSpecs.Rotation.x, DronePortSpecs.Rotation.y, DronePortSpecs.Rotation.z);
            gameObject.transform.localScale = DronePortSpecs.Scale;
            
        }

    }
}
