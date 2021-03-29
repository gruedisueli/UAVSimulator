using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Mapbox.VectorTile;
using Mapbox.Unity.MeshGeneration.Data;

using Newtonsoft.Json;

using Assets.Scripts.Serialization;
using Assets.Scripts.DataStructure;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Holds airspace info for a single feature in GIS data.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AirspaceFeature
    {
        [JsonProperty]
        private List<List<SerVect3f>> _polygonVerts = new List<List<SerVect3f>>();
        /// <summary>
        /// The set of polygon verts as originally found in GIS dataset, referenced off of "top left" of parent slippy tile.
        /// </summary>
        public List<List<SerVect3f>> PolygonVerts
        {
            get
            {
                return _polygonVerts;
            }
        }

        [JsonProperty]
        private float _lowerElev = 0;
        /// <summary>
        /// The bottom elevation of the airspace feature.
        /// </summary>
        public float LowerElev
        {
            get
            {
                return _lowerElev;
            }
        }

        [JsonProperty]
        private float _upperElev = 0;
        /// <summary>
        /// The top elevation of the airspace feature.
        /// </summary>
        public float UpperElev
        {
            get
            {
                return _upperElev;
            }
        }

        [JsonProperty]
        private AirspaceClass _class = AirspaceClass.Unset;
        /// <summary>
        /// The class of airspace this feature belongs to.
        /// </summary>
        public AirspaceClass Class
        {
            get
            {
                return _class;
            }
        }

        [JsonProperty]
        private string _id = "";
        /// <summary>
        /// Feature Id as found in GIS
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
        }

        [JsonProperty]
        private SerVect3f _origin = new SerVect3f();
        /// <summary>
        /// The origin as defined by parent slippy tile.
        /// </summary>
        public SerVect3f Origin
        {
            get
            {
                return _origin;
            }
        }

        public List<GameObject> Zones { get; private set; } = new List<GameObject>();

        public AirspaceFeature(List<List<Vector3>> points, Vector3 origin, AirspaceClass c, string id, float lowerElev, float upperElev)
        {
            foreach(var pList in points)
            {
                var vList = new List<SerVect3f>();
                foreach (var p in pList)
                {
                    vList.Add(new SerVect3f(p));
                }
                _polygonVerts.Add(vList);
            }
            _lowerElev = lowerElev;
            _upperElev = upperElev;
            _class = c;
            _id = id;
            _origin = new SerVect3f(origin);

            CreateGeometry();
        }

        public AirspaceFeature(AirspaceFeature f)
        {
            foreach(var vList in f.PolygonVerts)
            {
                var dupList = new List<SerVect3f>();
                foreach (var v in vList)
                {
                    dupList.Add(new SerVect3f(v));
                }
                _polygonVerts.Add(dupList);
            }
            _lowerElev = f.LowerElev;
            _upperElev = f.UpperElev;
            _class = f.Class;
            _id = f.Id;
            _origin = new SerVect3f(f.Origin);

            CreateGeometry();
        }

        /// <summary>
        /// Builds geometry that should be visualized.
        /// </summary>
        protected void CreateGeometry()
        {
            Vector3 disp = new Vector3(Origin.X, LowerElev, Origin.Z);//displacement vector that includes both origin and lower elevation adjustments to points.
            for (int i = 0; i < PolygonVerts.Count; i++)
            {
                var vList = PolygonVerts[i];
                Vector3[] arr = new Vector3[vList.Count];
                for (int j = 0; j < vList.Count; j++)
                {
                    arr[j] = vList[j].ToVector3();
                }
                Polygon pL = new Polygon(arr);
                Mesh m = null;
                //pL.Move(disp);
                if (UpperElev > LowerElev)
                {
                    m = pL.CreateExtrusion(UpperElev - LowerElev);
                }
                if (m == null)
                {
                    Debug.LogError("Failed to extrude airspace");
                    return;
                }
                string n = $"Airspace_Id{Id}_{i.ToString()}_Class{Class.ToString()}";
                var gO = new GameObject(n);
                var mF = gO.AddComponent<MeshFilter>();
                var mR = gO.AddComponent<MeshRenderer>();
                mF.mesh = m;
                gO.transform.position += disp;
            }
        }
    }
}
