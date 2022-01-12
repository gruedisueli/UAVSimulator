using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class TextElement : MonoBehaviour
    {
        public Text _text;
        public String _nameOverride = "";
        public ElementPropertyType _propertyType;

        /// <summary>
        /// Sets text as a value with the element id as the heading.
        /// </summary>
        public void SetTextAsValue(string value)
        {
            var prefix = _nameOverride == "" ? _propertyType.ToString() : _nameOverride;
            _text.text = prefix + ": " + value;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
