using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Vehicle_Control
{
    /// <summary>
    /// @Eunu comment
    /// </summary>
    public class VehicleControlSignal
    {
        public string signal;//@Eunu comment
        public GameObject from;//@Eunu comment
        public Vector3 vector;//@Eunu comment
        public Queue<GameObject> destinations;//@Eunu comment
        private GameObject gameObject;//@Eunu comment

        public VehicleControlSignal(GameObject from_i, string signal_i, Queue<GameObject> dest_i)
        {
            this.from = from_i;
            this.signal = signal_i;
            this.destinations = dest_i;
        }
        public VehicleControlSignal(GameObject from_i, string signal_i, Vector3 vec_i)
        {
            this.from = from_i;
            this.signal = signal_i;
            this.vector = vec_i;
        }
        public VehicleControlSignal(GameObject from_i, string signal_i)
        {
            this.from = from_i;
            this.signal = signal_i;
            this.vector = new Vector3(0,0,0);
        }
    }
    
}
