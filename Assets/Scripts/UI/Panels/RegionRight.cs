using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.Fields;
using Assets.Scripts.Environment;

namespace Assets.Scripts.UI.Panels
{
    public delegate void CityStatsChanged(string guid, CityOptions stats);

    public class RegionRight : MonoBehaviour
    {
        public EditableText cityName;
        public EditableInt statPopulation;
        public EditableInt statJobs;
        public EditableInt eastExt;
        public EditableInt westExt;
        public EditableInt northExt;
        public EditableInt southExt;
        public event CityStatsChanged statsChanged;
        public string _guid;

        private void Start()
        {
            if (statPopulation != null)
            {
                statPopulation.valueChanged += OnStatsChanged;
            }
            if (statJobs != null)
            {
                statJobs.valueChanged += OnStatsChanged;
            }
            if (cityName != null)
            {
                cityName.valueChanged += OnStatsChanged;
            }
            if (eastExt != null)
            {
                eastExt.valueChanged += OnStatsChanged;
            }
            if (westExt != null)
            {
                westExt.valueChanged += OnStatsChanged;
            }
            if (northExt != null)
            {
                northExt.valueChanged += OnStatsChanged;
            }
            if (southExt != null)
            {
                southExt.valueChanged += OnStatsChanged;
            }
        }

        public void SetCity(string guid, CityOptions stats)
        {
            _guid = guid;
            cityName.SetText(stats._name);
            statPopulation.SetValue(stats._population);
            statJobs.SetValue(stats._jobs);
            eastExt.SetValue(stats._eastExt);
            westExt.SetValue(stats._westExt);
            northExt.SetValue(stats._northExt);
            southExt.SetValue(stats._southExt);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void OnStatsChanged()
        {
            var s = new CityOptions();
            s._name = cityName.textValue;
            s._eastExt = eastExt.intValue;
            s._westExt = westExt.intValue;
            s._northExt = northExt.intValue;
            s._southExt = southExt.intValue;
            s._population = statPopulation.intValue;
            s._jobs = statJobs.intValue;
            statsChanged?.Invoke(_guid, s);
        }
    }
}
