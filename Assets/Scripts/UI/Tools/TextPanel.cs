using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class TextPanel : PanelBase
    {
        public Dictionary<ElementPropertyType, TextElement> TextElements { get; protected set; } = new Dictionary<ElementPropertyType, TextElement>();

        public virtual void Initialize()
        {
            TextElements = new Dictionary<ElementPropertyType, TextElement>();//refresh each time we initialize

            var tE = GetComponentsInChildren<TextElement>(true);
            if (tE == null || tE.Length == 0)
            {
                Debug.LogError("Text elements not found on text panel");
                return;
            }

            foreach (var e in tE)
            {
                try
                {
                    TextElements.Add(e._propertyType, e);
                }
                catch
                {
                    Debug.LogError("Given property is already present in dictionary on text panel, check that each text element has a unique property identifier");
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the bool value of a text element on this panel.
        /// </summary>
        public void SetTextElement(ElementPropertyType pT, bool value)
        {
            SetTextElement(pT, value.ToString());
        }


        /// <summary>
        /// Sets the float value of a text element on this panel.
        /// </summary>
        public void SetTextElement(ElementPropertyType pT, float value)
        {
            bool isMetric = EnvironManager.Instance.Environ.SimSettings.IsMetricUnits;
            string t = value.ToString("F2");
            switch (pT)
            {
                case ElementPropertyType.XScale:
                case ElementPropertyType.ZScale:
                case ElementPropertyType.Height:
                case ElementPropertyType.Radius:
                case ElementPropertyType.Bottom:
                case ElementPropertyType.Top:
                case ElementPropertyType.Elevation:
                {
                    t += isMetric ? " m" : " ft";
                    break;
                }
                case ElementPropertyType.MaxSpeed:
                case ElementPropertyType.TakeOffSpeed:
                case ElementPropertyType.LandingSpeed:
                case ElementPropertyType.CurrentSpeed:
                case ElementPropertyType.AverageSpeed:
                {
                    t += isMetric ? " km/hr" : " mi/hr";
                    break;
                }
                case ElementPropertyType.WaitTime:
                case ElementPropertyType.WaitTimer:
                {
                    t += " s";
                    break;
                }
            }

            SetTextElement(pT, t);
        }

        /// <summary>
        /// Sets the int value of a text element on this panel.
        /// </summary>
        public void SetTextElement(ElementPropertyType pT, int value)
        {
            SetTextElement(pT, value.ToString());
        }

        /// <summary>
        /// Sets the value of a text element on this panel.
        /// </summary>
        public void SetTextElement(ElementPropertyType pT, string value)
        {
            try
            {
                TextElements[pT].SetTextAsValue(value);
            }
            catch
            {
                Debug.LogError("Did not find property type in text panel to populate with provided value");
            }
        }
    }
}
