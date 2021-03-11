using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.Fields;
using Assets.Scripts.Environment;

namespace Assets.Scripts.UI.Tools
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
            cityName.SetText(stats.Name);
            statPopulation.SetValue(stats.Population);
            statJobs.SetValue(stats.Jobs);
            eastExt.SetValue(stats.EastExt);
            westExt.SetValue(stats.WestExt);
            northExt.SetValue(stats.NorthExt);
            southExt.SetValue(stats.SouthExt);
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
            s.Name = cityName.textValue;
            s.EastExt = eastExt.intValue;
            s.WestExt = westExt.intValue;
            s.NorthExt = northExt.intValue;
            s.SouthExt = southExt.intValue;
            s.Population = statPopulation.intValue;
            s.Jobs = statJobs.intValue;
            statsChanged?.Invoke(_guid, s);
        }
    }
}
