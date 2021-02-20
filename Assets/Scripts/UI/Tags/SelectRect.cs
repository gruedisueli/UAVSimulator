using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tags
{
    /// <summary>
    /// A selection rectangle to surround an area on screen.
    /// </summary>
    public class SelectRect : UIWorldTag
    {
        public Image _rectangle;

        /// <summary>
        /// Resize the rectangle
        /// </summary>
        public void SetExtents(Vector3 start, Vector3 end)
        {
            if (_rectTransform != null)
            {
                var c = (start + end) / 2;
                var xDim = Math.Abs(end.x - start.x);
                var yDim = Math.Abs(end.y - start.y);
                _rectTransform.anchoredPosition = c;
                _rectangle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, xDim);
                _rectangle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yDim);
            }
        }
    }
}
