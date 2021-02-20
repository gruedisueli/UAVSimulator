using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Fields
{
    public delegate void StatValueChanged();

    public abstract class EditableField : MonoBehaviour
    {
        public string statName;
        public InputField inputField;
        public Text statValueText;
        public Text statNameText;
        public event StatValueChanged valueChanged;

        public abstract void InputEntered();

        protected void OnValueChanged()
        {
            valueChanged?.Invoke();
        }

        private void Start()
        {
            if (statNameText != null)
            {
                statNameText.text = statName;
            }
        }
    }
}
