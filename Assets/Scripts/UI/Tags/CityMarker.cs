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
    public delegate void CityMarkerSelected(string guid, string cityName);

    public class CityMarker : UIWorldTag, IPointerClickHandler
    {
        //public LineRenderer _lineRend;
        public Image _square;
        public Text _text;
        public event CityMarkerSelected _markerSelected;
        public string _guid;

        private readonly Color colorDefault = new Color(1, 1, 1, 0.5f);
        //private UIWorldPt _worldCorners = new UIWorldPt[5];
        private UIWorldPt _minPt;
        private UIWorldPt _maxPt;
        private UIWorldPt _centerPt;

        protected override void CustomStart()
        {
            _square.color = colorDefault;
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
            //_square.rectTransform.sizeDelta = new Vector2(xDim, yDim);
            _square.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, xDim);
            _square.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yDim);
            
            _square.rectTransform.anchoredPosition = _centerPt.GetScreenPt();
            //for (int i = 0; i < _worldCorners.Length; i++)
            //{
            //    if (_worldCorners[i] != null)
            //    {
            //        var p = _worldCorners[i].GetScreenPt();
            //        _square.rectTransform.sizeDelta = new Vector2(p.x, p.y);
            //        //_lineRend.SetPosition(i, new Vector3(p.x, p.y, 0));
            //    }
            //}
        }

        public void SetGuid(string guid)
        {
            _guid = guid;
        }

        public void SetName(string n)
        {
            if (_text != null)
            {
                _text.text = n;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _markerSelected.Invoke(_guid, _text.text);
        }

        /// <summary>
        /// Sets color of marker.
        /// </summary>
        public void SetColor(Color c)
        {
            _square.color = c;
        }

        /// <summary>
        /// Resets color of marker to default.
        /// </summary>
        public void ResetColor()
        {
            _square.color = colorDefault;
        }

        /// <summary>
        /// Sets line coordinates in 3d space using a set of x-range and z-range values (in that order)
        /// Note: input array must have x-range in first index and z in second.
        /// Each of these secondary arrays must contain two elements to work properly.
        /// </summary>
        public void SetExtents(float[][] rangesXZ)
        {
            if (rangesXZ.Length != 2 || rangesXZ[0] == null || rangesXZ[0].Length != 2 || rangesXZ[1] == null || rangesXZ[1].Length != 2)
            {
                print("Incorrect dimension for city box extents");
                return;
            }
            
            //_worldCorners[0] = new UIWorldPt(new Vector3(rangesXZ[0][0], 0, rangesXZ[1][0]));
            //_worldCorners[1] = new UIWorldPt(new Vector3(rangesXZ[0][0], 0, rangesXZ[1][1]));
            //_worldCorners[2] = new UIWorldPt(new Vector3(rangesXZ[0][1], 0, rangesXZ[1][1]));
            //_worldCorners[3] = new UIWorldPt(new Vector3(rangesXZ[0][1], 0, rangesXZ[1][0]));
            //_worldCorners[4] = new UIWorldPt(new Vector3(rangesXZ[0][0], 0, rangesXZ[1][0]));

            _minPt = new UIWorldPt(new Vector3(rangesXZ[0][0], 0, rangesXZ[1][0]));
            _maxPt = new UIWorldPt(new Vector3(rangesXZ[0][1], 0, rangesXZ[1][1]));
            _centerPt = new UIWorldPt((_minPt._worldPos + _maxPt._worldPos) / 2);
        }
    }
}
