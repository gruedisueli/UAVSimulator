using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            SetTextElement(pT, value.ToString("F2"));
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
