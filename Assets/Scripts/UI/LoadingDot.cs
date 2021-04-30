using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI
{
    public class LoadingDot : MonoBehaviour
    {
        float _activeFrames = 0;

        private void Update()
        {
            float scale = 0.25f * (float)Math.Sin(_activeFrames / 50) + 1;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
            _activeFrames++;
        }
    }
}
