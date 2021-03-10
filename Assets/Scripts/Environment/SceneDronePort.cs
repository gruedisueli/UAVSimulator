using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// The instance of a drone port in the scene itself.
    /// </summary>
    public class SceneDronePort : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public DronePortBase DronePortSpecs { get; private set; }
        public DronePortControl DronePortCtrl { get; private set; }

        public void Initialize(DronePortBase dP, string guid)
        {
            Guid = guid;
            DronePortSpecs = dP;
            gameObject.name = "DronePort_" + dP.Type;
            gameObject.tag = "DronePort";
            gameObject.layer = dP.Layer;
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
            gameObject.transform.position = DronePortSpecs.Position;
            gameObject.transform.rotation = Quaternion.Euler(DronePortSpecs.Rotation.x, DronePortSpecs.Rotation.y, DronePortSpecs.Rotation.z);
            gameObject.transform.localScale = DronePortSpecs.Scale;
        }
    }
}
