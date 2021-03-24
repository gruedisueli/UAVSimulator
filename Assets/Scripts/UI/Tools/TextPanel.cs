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
    }
}
