using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class ErrorDialog : MonoBehaviour
    {
        public double _visibleTime = 3;//in seconds
        public void Activate()
        {
            gameObject.SetActive(true);
            StartCoroutine(Deactivate());
        }

        private IEnumerator Deactivate()
        {
            var t0 = Time.unscaledTimeAsDouble;
            while (Time.unscaledTimeAsDouble - t0 < _visibleTime)
            {
                yield return new WaitForEndOfFrame();
            }
            gameObject.SetActive(false);
        }
    }
}
