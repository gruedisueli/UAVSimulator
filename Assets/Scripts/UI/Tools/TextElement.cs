using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public abstract class TextElement : MonoBehaviour
    {
        public Text _text;
        public ElementPropertyType _propertyType;

        /// <summary>
        /// Sets text as a value with the element id as the heading.
        /// </summary>
        public void SetTextAsValue(string value)
        {
            _text.text = _propertyType.ToString() + ": " + value;
        }
    }
}
