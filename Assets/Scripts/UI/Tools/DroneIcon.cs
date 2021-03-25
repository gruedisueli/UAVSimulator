using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Tools
{
    public class DroneIcon : SceneIconBase
    {
        public Vehicle Drone { get; protected set; }

        public void Initialize(Vehicle _drone)
        {
            Drone = _drone;
            _follower.Initialize(_drone.gameObject);
            Drone.OnDroneTakeOff += OnTakeOff;
            Drone.OnDroneParking += OnParking;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Drone.OnDroneTakeOff -= OnTakeOff;
            Drone.OnDroneParking -= OnParking;
        }

        public void OnTakeOff(object sender, System.EventArgs args)
        {
            SetActive(true);
        }

        public void OnParking(object sender, System.EventArgs args)
        {
            SetActive(false);
        }
    }
}
