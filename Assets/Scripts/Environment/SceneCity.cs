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
        public CityOptions CitySpecs { get; protected set; }

        private readonly Color _colorDefault = new Color(1, 1, 1, 0.5f);
        private readonly Color _selectedColor = Color.red;
        private MeshRenderer _renderer;

        public void Initialize(string guid, CityOptions citySpecs)
        {
            Guid = guid;
            CitySpecs = citySpecs;
            _renderer = gameObject.GetComponent<MeshRenderer>();
            _renderer.materials[0].color = _colorDefault;

            UpdateGameObject();
        }

        public override void SetSelectedState(bool isSelected)
        {
            _renderer.materials[0].color = isSelected ? _selectedColor : _colorDefault;
        }

        public override void UpdateGameObject()
        {
            var c = CitySpecs;
            float cityTileSideLength = (float)UnitUtils.GetCityTileSideLength();
            var tile = UnitUtils.GetLocalCityTileCoords(c.WorldPos, c.RegionTileWorldCenter, cityTileSideLength);
            var extents = UnitUtils.TileCoordsToWorldExtents(tile, c.RegionTileWorldCenter, cityTileSideLength, c.EastExt, c.WestExt, c.NorthExt, c.SouthExt);
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
