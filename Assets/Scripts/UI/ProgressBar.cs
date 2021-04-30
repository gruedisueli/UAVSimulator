using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ProgressBar : MonoBehaviour
    {
        public RectTransform _completionBar;
        public Text _description;
        private float _overallWidth;

        private void Awake()
        {
            _overallWidth = _completionBar.sizeDelta.x;
        }

        /// <summary>
        /// Sets initial conditions for progress bar.
        /// </summary>
        public void Init(string desc = "")
        {
            SetCompletion(0.01f);
            if (desc == "")
            {
                _description.gameObject.SetActive(false);
            }
            else
            {
                _description.text = desc;
            }
        }

        /// <summary>
        /// Sets completion value of progress bar.
        /// </summary>
        public void SetCompletion(float proportion)
        {
            _completionBar.sizeDelta = new Vector2(_overallWidth * proportion, _completionBar.sizeDelta.y);
        }
    }
}
