using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Tools
{
    public class DroneInfoPanel : ElementInfoPanel
    {
        protected DroneIcon _icon;
        protected DroneBase _drone;

        public override void Initialize(GameObject drone)
        {
            base.Initialize(drone);

            if (_icon != null) //clear out existing selected element
            {
                _icon.SetSelected(false);
            }

            _icon = drone.GetComponentInChildren<DroneIcon>(true);
            if (_icon == null)
            {
                Debug.LogError("Drone Icon Component not found in children of drone game object");
                return;
            }
            _drone = _icon.Drone;

            SetActive(true);
            StartCoroutine(UpdateInfoPanel());
        }

        protected override void ModifyTextValues(IModifyElementArgs args)
        {
            throw new NotImplementedException();
        }

        protected override void Close(object sender, System.EventArgs args)
        {
            base.Close(sender, args);

            _icon?.SetSelected(false);
        }

        /// <summary>
        /// Coroutine called when info panel is active.
        /// </summary>
        protected IEnumerator UpdateInfoPanel()
        {
            bool isMetric = EnvironManager.Instance.Environ.SimSettings.IsMetricUnits;
            while (_icon.IsSelected)
            {
                yield return new WaitForSeconds(0.25f);//prevent excessive updating

                //get stuff from the drone to report
                var i = _infoPanel;
                var d = _drone;
                float maxSpeed, takeOffSpeed, landingSpeed, currentSpeed, elevation;
                if (isMetric)
                {
                    maxSpeed = UnitUtils.MetersPerSecondToKilometersPerHour(d.MaxSpeed);
                    takeOffSpeed = UnitUtils.MetersPerSecondToKilometersPerHour(d.TakeOffSpeed);
                    landingSpeed = UnitUtils.MetersPerSecondToKilometersPerHour(d.LandingSpeed);
                    currentSpeed = UnitUtils.MetersPerSecondToKilometersPerHour(d.CurrentSpeed);
                    elevation = d.Elevation;
                }
                else
                {
                    maxSpeed = UnitUtils.MetersPerSecondToMilesPerHour(d.MaxSpeed);
                    takeOffSpeed = UnitUtils.MetersPerSecondToMilesPerHour(d.TakeOffSpeed);
                    landingSpeed = UnitUtils.MetersPerSecondToMilesPerHour(d.LandingSpeed);
                    currentSpeed = UnitUtils.MetersPerSecondToMilesPerHour(d.CurrentSpeed);
                    elevation = UnitUtils.MetersToFeet(d.Elevation);
                }
                i.SetTextElement(ElementPropertyType.Id, d.Id);
                i.SetTextElement(ElementPropertyType.Type, d.DroneType);
                //i.SetTextElement(ElementPropertyType.Capacity, d.capacity);
                i.SetTextElement(ElementPropertyType.MaxSpeed, maxSpeed);
                //i.SetTextElement(ElementPropertyType.YawSpeed, d.yawSpeed);
                i.SetTextElement(ElementPropertyType.TakeOffSpeed, takeOffSpeed);
                i.SetTextElement(ElementPropertyType.LandingSpeed, landingSpeed);
                //i.SetTextElement(ElementPropertyType.Range, d.range);
                ////i.SetTextElement(ElementPropertyType.Emission, d.emission)
                ////i.SetTextElement(ElementPropertyType.Noise, d.noise);
                ////i.SetTextElement(ElementPropertyType.Position, d.currentLocation);
                ////i.SetTextElement(ElementPropertyType.TargetPosition, d.currentTargetPosition);
                i.SetTextElement(ElementPropertyType.CurrentSpeed, currentSpeed);
                i.SetTextElement(ElementPropertyType.Elevation, elevation);
                //i.SetTextElement(ElementPropertyType.Origin, d.origin);
                ////i.SetTextElement(ElementPropertyType.Description, d.destination);
                ////i.SetTextElement(ElementPropertyType.DestinationList, d.destinationList);
                //i.SetTextElement(ElementPropertyType.Separation, d.separation);
                i.SetTextElement(ElementPropertyType.State, d.State);
                //i.SetTextElement(ElementPropertyType.PlaceInQueue, d.placeInQueue);
                //i.SetTextElement(ElementPropertyType.ToPark, d.toPark);
                //i.SetTextElement(ElementPropertyType.MoveForward, d.moveForward);
                //i.SetTextElement(ElementPropertyType.IsUTM, d.isUTM);
                //i.SetTextElement(ElementPropertyType.IsBackgroundDrone, d.isBackgroundDrone);
                i.SetTextElement(ElementPropertyType.WaitTimer, d.WaitTimer);
                //i.SetTextElement(ElementPropertyType.WaitTime, d.waitTime);

                yield return null;
            }
        }
    }
}
