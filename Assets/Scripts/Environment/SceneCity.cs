using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.UI.Tags;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class SceneCity : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public CityOptions CitySpecs { get; protected set; }

        private Material _defaultMat;
        private Material _selMat;

        public void Initialize(string guid, CityOptions citySpecs)
        {
            Guid = guid;
            CitySpecs = citySpecs;
            _defaultMat = Resources.Load<Material>("Materials/CityDefault");
            _selMat = Resources.Load<Material>("Materials/CitySelected");
            if (_renderers == null)
            {
                _renderers = GetComponentsInChildren<MeshRenderer>(true);
                if (_renderers == null)
                {
                    Debug.LogError("Mesh renderer not found on city element");
                }
                else
                {
                    _renderers[0].material = _defaultMat;
                }
            }
            else
            {
                _renderers[0].material = _defaultMat;
            }

            UpdateGameObject();
            MakeSelectable();
        }

        public override void SetSelectedState(bool isSelected)
        {
            //base.SetSelectedState(isSelected);
            _renderers[0].material = isSelected ? _selMat : _defaultMat;
        }

        public override void UpdateGameObject()
        {
            var extents = UnitUtils.GetCityExtents(CitySpecs);
            float xRange = Math.Abs(extents[0][0] - extents[0][1]);
            float zRange = Math.Abs(extents[1][0] - extents[1][1]);
            gameObject.transform.localScale = new Vector3(xRange, 1000, zRange);//setting large y range to get over terrain, for now.

            //get center of game object (different from city center, which is the point from which extents are measured.)
            var minPt = new Vector3(extents[0][0], 0, extents[1][0]);
            var maxPt = new Vector3(extents[0][1], 0, extents[1][1]);
            var centerPt = (minPt + maxPt) / 2;

            gameObject.transform.position = centerPt;
        }


        
    }
}
