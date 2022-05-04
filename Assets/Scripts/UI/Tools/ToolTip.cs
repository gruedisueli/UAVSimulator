using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject _toolTip;
        public float _timeTrigger = 1;//in seconds, how long you have to hover mouse before it shows up.
        private bool _entered = false;
        private float _timeEntered = 0;

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.name == "Tooltip")
                {
                    _toolTip = child.gameObject;
                    break;
                }
            }

            if (_toolTip == null)
            {
                Debug.LogError("Could not find tooltip child");
            }
        }
        public void OnPointerEnter(PointerEventData d)
        {
            if (EnvironManager.Instance.Environ.SimSettings.DisplayTooltips)
            {
                _entered = true;
                _timeEntered = Time.unscaledTime;
                StartCoroutine(DisplayTooltip());
            }
        }

        public void OnPointerExit(PointerEventData d)
        {
            if (EnvironManager.Instance.Environ.SimSettings.DisplayTooltips)
            {
                _entered = false;
            }
        }

        private IEnumerator DisplayTooltip()
        {
            while (_entered && Time.unscaledTime - _timeEntered < _timeTrigger)
            {
                yield return new WaitForEndOfFrame();
            }

            if (!_entered) yield break;
            _toolTip.SetActive(true);
            while (_entered)
            {
                yield return new WaitForEndOfFrame();
            }
            _toolTip.SetActive(false);

        }
    }
}
