using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.SimulatorCore;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Tools
{
    public class SimulationInfoPanel : PanelBase
    {
        protected SimulationAnalyzer _analyzer;
        protected TextPanel _infoPanel;
        protected VehicleControlSystem _control;
        protected int _frameCounter = 0;
        protected int _updateRate = 15;//update stats every x frames

        protected override void Awake()
        {
            base.Awake();

            _analyzer = FindObjectOfType<SimulationAnalyzer>(true);
            if (_analyzer == null)
            {
                Debug.LogError("Could not find simulation analyzer in scene");
                return;
            }
            _control = FindObjectOfType<VehicleControlSystem>(true);
            if (_control == null)
            {
                Debug.LogError("Could not find vehicle control system in scene");
                return;
            }
            _infoPanel = FindCompInChildren<TextPanel>();
            _infoPanel?.Initialize();
            SetText();
        }

        private void Update()
        {
            if (_control.playing && _frameCounter >= _updateRate) //no sense in updating stats when paused.
            {
                _frameCounter = -1;
                SetText();
            }
            _frameCounter++;
        }

        /// <summary>
        /// Sets text values.
        /// </summary>
        private void SetText()
        {
            var i = _infoPanel;
            var s = _analyzer;
            bool isMetric = EnvironManager.Instance.Environ.SimSettings.IsMetricUnits;
            var speed = isMetric ? UnitUtils.MetersPerSecondToKilometersPerHour(s.averageSpeed) : UnitUtils.MetersPerSecondToMilesPerHour(s.averageSpeed);
            i.SetTextElement(ElementPropertyType.AverageSpeed, speed);
            i.SetTextElement(ElementPropertyType.Throughput, s.throughput);
            //i.SetTextElement(ElementPropertyType.GrossEnergyConsumption, s.grossEnergyConsumption);
            //i.SetTextElement(ElementPropertyType.GrossEmission, s.grossEmission);
            i.SetTextElement(ElementPropertyType.HighNoiseBuildings, s.highNoiseBuildings?.Count ?? 0);
            i.SetTextElement(ElementPropertyType.MediumNoiseBuildings, s.mediumNoiseBuildings?.Count ?? 0);
            i.SetTextElement(ElementPropertyType.LowNoiseBuildings, s.lowNoiseBuildings?.Count ?? 0);
            i.SetTextElement(ElementPropertyType.FlyingDrones, s.flyingDrones?.Count ?? 0);
            i.SetTextElement(ElementPropertyType.CongestedCorridors, s.congestedCorridors?.Count ?? 0);
            i.SetTextElement(ElementPropertyType.CongestedParkingStructures, s.congestedParkingStructures?.Count ?? 0);
            i.SetTextElement(ElementPropertyType.CongestedDronePorts, s.congestedDronePorts?.Count ?? 0);
        }
    }
}
