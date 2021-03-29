using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

using Mapbox.VectorTile;
using Mapbox.Unity.MeshGeneration.Data;

using Assets.Scripts.MapboxCustom;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Holds a set of features for a specific slippy tile.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AirspaceTile
    {
        [JsonProperty]
        private string _slippyTileName = "";
        /// <summary>
        /// The name of the slipoy tile that this tile is referencing.
        /// </summary>
        public string SlippyTileName
        {
            get
            {
                return _slippyTileName;
            }
        }

        [JsonProperty]
        private Dictionary<string, AirspaceFeature> _features = new Dictionary<string, AirspaceFeature>();
        /// <summary>
        /// List of the features that are found on this tile.
        /// </summary>
        public Dictionary<string, AirspaceFeature> Features
        {
            get
            {
                return _features;
            }
        }

        public AirspaceTile(VectorTileLayer layer, UnityTile tile)
        {
            //var regionTileSideLength = UnitUtils.GetRegionTileSideLength();
            //float rTHalfSide = regionTileSideLength / 2;
            var tileCenter = tile.gameObject.transform.position;
            //tileCenter = new Vector3(tileCenter.x, 0, tileCenter.z);//remove vertical component.
            //Vector3 tileOrigin = new Vector3(tileCenter.x - regionTileSideLength, 0, tileCenter.z + regionTileSideLength);//should be "upper left" of tile.
            for (int i = 0; i < layer.FeatureCount(); i++)
            {
                var feature = layer.GetFeature(i);
                var f = new VectorFeatureUnity(feature, tile, layer.Extent);
                //parse class. some classes we can ignore for now.
                AirspaceClass aC;
                if (f.Properties.ContainsKey("CLASS"))
                {
                    var c = f.Properties["CLASS"] as string;
                    if (c.Contains("B"))
                    {
                        aC = AirspaceClass.B;
                    }
                    else if (c.Contains("C"))
                    {
                        aC = AirspaceClass.C;
                    }
                    else if (c.Contains("D"))
                    {
                        aC = AirspaceClass.D;
                    }
                    else if (c.Contains("E"))
                    {
                        continue;// aC = AirspaceClass.E;
                    }
                    else
                    {
                        continue; //ignore for now.
                    }
                }
                else
                {
                    Debug.LogError("Airspace feature does not contain CLASS property");
                    continue;
                }
                float lowerValFT;
                if (f.Properties.ContainsKey("LOWER_VAL"))
                {
                    if(!float.TryParse(f.Properties["LOWER_VAL"] as string, out lowerValFT))
                    {
                        Debug.LogError("Was not able to parse LOWER_VAL of airspace feature");
                        continue;
                    }
                }
                else
                {
                    Debug.LogError("Airspace feature does not contain LOWER_VAL property");
                    continue;
                }
                float upperValFT;
                if (f.Properties.ContainsKey("UPPER_VAL"))
                {
                    if (!float.TryParse(f.Properties["UPPER_VAL"] as string, out upperValFT))
                    {
                        Debug.LogError("Was not able to parse UPPER_VAL of airspace feature");
                        continue;
                    }
                }
                else
                {
                    Debug.LogError("Airspace feature does not contain UPPER_VAL property");
                    continue;
                }

                //build mesh data.
                MeshData mD = new MeshData();
                mD.TileRect = tile.Rect;
                FOA_PolygonMesh pM = new FOA_PolygonMesh();
                pM.Run(f, mD);


                float upperValM = upperValFT * 0.3048f;
                float lowerValM = lowerValFT * 0.3048f;

                _features.Add(feature.Id.ToString(), new AirspaceFeature(f.Points, tileCenter, aC, feature.Id.ToString(), lowerValM, upperValM));
            }
        }
    }
}
