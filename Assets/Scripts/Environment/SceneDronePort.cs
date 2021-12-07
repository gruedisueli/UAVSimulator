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
    /// The instance of a drone port in the scene itself.
    /// </summary>
    public class SceneDronePort : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override bool Is2D { get; protected set; } = false;
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public override bool IsSelectable { get; protected set; } = false;
        public DronePortBase DronePortSpecs { get; private set; }
        public DronePortControl DronePortCtrl { get; private set; }

        public void Initialize(DronePortBase dP, string guid, bool is2D, Canvas canvas, bool isSelectable)
        {
            Guid = guid;
            Is2D = is2D;
            SceneCanvas = canvas;
            if (is2D) Sprite2d = Instantiate(EnvironManager.Instance.PortSpritePrefab, SceneCanvas.transform);
            IsSelectable = isSelectable;
            if (IsSelectable) MakeSelectable();
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
        }

        /// <summary>
        /// Synchronizes the game object with latest info in environment drone port info.
        /// </summary>
        public override void UpdateGameObject()
        {
            if (Is2D)
            {
                var tag = Sprite2d.GetComponent<UIWorldTag>();
                tag?.SetWorldPos(DronePortSpecs.Position);
            }
            gameObject.transform.position = DronePortSpecs.Position;
            gameObject.transform.rotation = Quaternion.Euler(DronePortSpecs.Rotation.x, DronePortSpecs.Rotation.y, DronePortSpecs.Rotation.z);
            gameObject.transform.localScale = DronePortSpecs.Scale;
            
        }
    }
}
