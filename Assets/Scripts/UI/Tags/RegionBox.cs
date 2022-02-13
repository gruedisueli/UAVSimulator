using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Tags
{
    public class RegionBox : UIWorldTag
    {
        public Image _square;

        private UIWorldPt _minPt;
        private UIWorldPt _maxPt;
        private UIWorldPt _centerPt;

        public void SetWorldExtents(Vector3 center, float inflationDist)
        {
            var minExt = new Vector3(center.x - inflationDist, 0, center.z - inflationDist);
            var maxExt = new Vector3(center.x + inflationDist, 0, center.z + inflationDist);
            _minPt = new UIWorldPt(minExt);
            _maxPt = new UIWorldPt(maxExt);
            _centerPt = new UIWorldPt(center);
            SetWorldPos(center);
        }

        protected override void CustomUpdate()
        {
            if (_square == null)
            {
                return;
            }

            var min = _minPt.GetScreenPt();
            var max = _maxPt.GetScreenPt();
            float xDim = Math.Abs(max.x - min.x);
            float yDim = Math.Abs(max.y - min.y);

            _square.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, xDim);
            _square.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yDim);
            
            _square.rectTransform.anchoredPosition = _centerPt.GetScreenPt();
        }
    }
}
