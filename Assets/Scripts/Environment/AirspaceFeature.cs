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
        private List<SerVect3f> _vertices = new List<SerVect3f>();
        public List<SerVect3f> Vertices
        {
            get
            {
                return _vertices;
            }
        }

        [JsonProperty]
        private List<SerVect3f> _normals = new List<SerVect3f>();
        public List<SerVect3f> Normals
        {
            get
            {
                return _normals;
            }
        }

        [JsonProperty]
        private List<SerVect4f> _tangents = new List<SerVect4f>();
        public List<SerVect4f> Tangents
        {
            get
            {
                return _tangents;
            }
        }

        [JsonProperty]
        private List<List<SerVect2f>> _uv = new List<List<SerVect2f>>();
        public List<List<SerVect2f>> UV
        {
            get
            {
                return _uv;
            }
        }

        [JsonProperty]
        private List<List<int>> _triangles = new List<List<int>>();
        public List<List<int>> Triangles
        {
            get
            {
                return _triangles;
            }
        }

        [JsonProperty]
        private List<int> _edges = new List<int>();
        public List<int> Edges
        {
            get
            {
                return _edges;
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

        [JsonProperty]
        private List<SerVect3f> _orderedPoints = new List<SerVect3f>();
        /// <summary>
        /// Points as ordered prior to mesh generation.
        /// </summary>
        public List<SerVect3f> OrderedPoints
        {
            get
            {
                return _orderedPoints;
            }
        }

        public GameObject Zone { get; private set; } = null;

        public AirspaceFeature()
        {

        }

        public AirspaceFeature(MeshData meshData, Vector3 origin, List<List<Vector3>> points, AirspaceClass c, string id, float lowerElev, float upperElev)
        {
            _vertices = ConverToSerVect3f(meshData.Vertices);
            _normals = ConverToSerVect3f(meshData.Normals);
            _uv = new List<List<SerVect2f>>();
            foreach (var uvList in meshData.UV)
            {
                var uL = new List<SerVect2f>();
                foreach(var u in uvList)
                {
                    uL.Add(new SerVect2f(u));
                }
                _uv.Add(uL);
            }
            _tangents = ConverToSerVect4f(meshData.Tangents);
            _triangles = new List<List<int>>();
            foreach(var tList in meshData.Triangles)
            {
                List<int> tS = new List<int>();
                foreach(var t in tList)
                {
                    tS.Add(t);
                }
                _triangles.Add(tS);
            }
            _edges = meshData.Edges;

            _lowerElev = lowerElev;
            _upperElev = upperElev;
            _class = c;
            _id = id;
            _origin = new SerVect3f(origin);

            if (points.Count == 0)
            {
                Debug.LogError("No ordered points supplied for airspace feature");
                return;
            }
            foreach(var p in points[0])//only supporting one sub-list for now.
            {
                _orderedPoints.Add(new SerVect3f(p));
            }

            CreateGeometry();
        }

        public AirspaceFeature(AirspaceFeature f)
        {
            foreach(var v in f.Vertices)
            {
                _vertices.Add(new SerVect3f(v));
            }
            foreach(var n in f.Normals)
            {
                _normals.Add(new SerVect3f(n));
            }
            foreach(var uList in f.UV)
            {
                var uL = new List<SerVect2f>();
                foreach(var u in uList)
                {
                    uL.Add(new SerVect2f(u));
                }
                _uv.Add(uL);
            }
            foreach(var t in f.Tangents)
            {
                _tangents.Add(new SerVect4f(t));
            }
            foreach(var tList in f.Triangles)
            {
                var tL = new List<int>();
                foreach(var t in tList)
                {
                    tL.Add(t);
                }
                _triangles.Add(tL);
            }
            foreach(var e in f.Edges)
            {
                _edges.Add(e);
            }

            _lowerElev = f.LowerElev;
            _upperElev = f.UpperElev;
            _class = f.Class;
            _id = f.Id;
            _origin = new SerVect3f(f.Origin);

            foreach(var p in f.OrderedPoints)
            {
                _orderedPoints.Add(new SerVect3f(p));
            }

            CreateGeometry();
        }

        private List<SerVect3f> ConverToSerVect3f(List<Vector3> origList)
        {
            List<SerVect3f> finalList = new List<SerVect3f>();
            foreach(var v in origList)
            {
                finalList.Add(new SerVect3f(v));
            }
            return finalList;
        }

        private List<SerVect4f> ConverToSerVect4f(List<Vector4> origList)
        {
            List<SerVect4f> finalList = new List<SerVect4f>();
            foreach (var v in origList)
            {
                finalList.Add(new SerVect4f(v));
            }
            return finalList;
        }

        private List<SerVect2f> ConverToSerVect2f(List<Vector2> origList)
        {
            List<SerVect2f> finalList = new List<SerVect2f>();
            foreach (var v in origList)
            {
                finalList.Add(new SerVect2f(v));
            }
            return finalList;
        }

        /// <summary>
        /// Builds geometry that should be visualized. Yscale useful for exaggeration of vertical height for visual clarity due to tendency of airspace features to be many times wider than they are tall.
        /// </summary>
        public void CreateGeometry(float yScale = 1.0f)
        {
            #region GENERAL

            string gOName = $"Airspace_Id{Id}_Class{Class.ToString()}_{LowerElev.ToString()}ft-{UpperElev.ToString()}";
            if (UpperElev <= LowerElev)
            {
                Debug.LogError($"Not building {gOName} because it lacks a valid height range");
                return;
            }
            var scaledUpperElev = UpperElev * yScale;
            var scaledLowerElev = LowerElev * yScale;

            var height = scaledUpperElev - scaledLowerElev;
            Vector3 disp = new Vector3(Origin.X, LowerElev, Origin.Z);//displacement vector that includes both origin and lower elevation adjustments to points.
            Vector3 heightV = new Vector3(0, height, 0);

            #endregion

            #region TOP/BOTTOM MESHES

            Mesh botMesh = new Mesh();
            Mesh topMesh = new Mesh();
            List<Vector3> botVerts = new List<Vector3>();
            List<Vector3> topVerts = new List<Vector3>();
            foreach (var v in Vertices)
            {
                botVerts.Add(v.ToVector3());
                topVerts.Add(v.ToVector3() + heightV);
            }
            List<Vector3> botNorms = new List<Vector3>();
            List<Vector3> topNorms = new List<Vector3>();
            foreach (var n in Normals)
            {
                //first check direction
                var norm = n.ToVector3();
                if (Vector3.Angle(Vector3.down, norm) > 90)
                {
                    norm *= -1;
                }
                botNorms.Add(norm);
                topNorms.Add(norm * -1);
            }
            List<Vector4> tangs = new List<Vector4>();
            foreach(var t in Tangents)
            {
                tangs.Add(t.ToVector4());
            }
            List<List<Vector2>> uvs = new List<List<Vector2>>();
            foreach(var uL in UV)
            {
                List<Vector2> uvList = new List<Vector2>();
                foreach(var u in uL)
                {
                    uvList.Add(u.ToVector2());
                }
                uvs.Add(uvList);
            }
            botMesh.SetVertices(botVerts);
            topMesh.SetVertices(topVerts);
            botMesh.SetNormals(botNorms);
            topMesh.SetNormals(topNorms);
            botMesh.SetTangents(tangs);
            topMesh.SetTangents(tangs);
            for(int i = 0; i < Triangles.Count; i++)
            {
                botMesh.SetTriangles(Triangles[i], i);
                topMesh.SetTriangles(Triangles[i], i);
            }
            for (int i = 0; i < uvs.Count; i++)
            {
                botMesh.SetUVs(i, uvs[i]);
                topMesh.SetUVs(i, uvs[i]);
            }

            #endregion

            #region SIDE MESH

            List<Vector3> oPts = new List<Vector3>();
            foreach(var p in _orderedPoints)
            {
                oPts.Add(p.ToVector3());
            }

            //side mesh code adapted from Mapbox.Unity.MeshGeneration.Modifiers.HeightModifier
            Mesh sideMesh = new Mesh();
            bool isCW = IsClockwise(oPts);

            int c = oPts.Count * 2;
            Vector3[] localVerts = new Vector3[c];
            Vector3[] localNorms = new Vector3[c];
            Vector4[] localTangs = new Vector4[c];
            List<Vector2> localUVs = new List<Vector2>();
            List<int> localTris = new List<int>();
            int vCt = oPts.Count;
            int sVCt = vCt * 2;
            for (int i = 0; i < vCt; i ++)
            {
                //typically we are only adding two vertices and their components on an iteration, except for index 0
                int v = i;
                int v2 = i < vCt - 1 ? i + 1 : 0;
                var v1B = oPts[v];
                var v2B = oPts[v2];
                var v1T = v1B + heightV;
                var v2T = v2B + heightV;

                int sV0 = v * 2;//side vertex index
                int sV1 = sV0 + 1 < sVCt ? sV0 + 1 : sV0 + 1 - sVCt;
                int sV2 = sV0 + 2 < sVCt ? sV0 + 2 : sV0 + 2 - sVCt;
                int sV3 = sV0 + 3 < sVCt ? sV0 + 3 : sV0 + 3 - sVCt;
                if (v == 0) //only first segment requires adding of these verts...
                {
                    localVerts[sV0] = new Vector3(v1B.x, v1B.y, v1B.z);
                    localVerts[sV1] = new Vector3(v1T.x, v1T.y, v1T.z);
                }
                if (v < vCt - 1)//don't add any verts on last step.
                {
                    localVerts[sV2] = new Vector3(v2B.x, v2B.y, v2B.z);
                    localVerts[sV3] = new Vector3(v2T.x, v2T.y, v2T.z);
                }
                float d = (v2B - v1B).magnitude;
                Vector3 norm = Vector3.Normalize(Vector3.Cross(v2B - v1B, v1T - v1B));
                if (!isCW)
                {
                    norm *= -1;
                }
                if (v == 0)
                {
                    localNorms[sV0] = norm;
                    localNorms[sV1] = norm;
                }
                if (v < vCt - 1)
                {
                    localNorms[sV2] = norm;
                    localNorms[sV3] = norm;
                }
                Vector3 wallDir = (v2B - v1B).normalized;
                if (v == 0)
                {
                    localTangs[sV0] = wallDir;
                    localTangs[sV1] = wallDir;
                }
                if (v < vCt - 1)
                {
                    localTangs[sV2] = wallDir;
                    localTangs[sV3] = wallDir;
                }
                if (v == 0)
                {
                    localUVs.Add(new Vector2(0, 0));
                    localUVs.Add(new Vector2(0, -height));
                }
                if (v < vCt - 1)
                {
                    localUVs.Add(new Vector2(d, 0));
                    localUVs.Add(new Vector2(d, -height));
                }

                //always add both triangles
                localTris.Add(sV0);
                localTris.Add(sV1);
                localTris.Add(sV2);

                localTris.Add(sV1);
                localTris.Add(sV3);
                localTris.Add(sV2);
            }
            sideMesh.SetVertices(localVerts);
            sideMesh.SetNormals(localNorms);
            sideMesh.SetTangents(localTangs);
            sideMesh.SetUVs(0, localUVs);
            sideMesh.SetTriangles(localTris, 0);

            #endregion

            #region FINISHING

            Mesh airspaceMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[3];
            combine[0].mesh = botMesh;
            combine[1].mesh = sideMesh;
            combine[2].mesh = topMesh;
            airspaceMesh.CombineMeshes(combine, true, false);

            GameObject gO = new GameObject(gOName);
            var mR = gO.AddComponent<MeshRenderer>();
            var mF = gO.AddComponent<MeshFilter>();
            mR.sharedMaterial = UnityEngine.Object.Instantiate(EnvironManager.Instance.RestrictionZoneMaterial);
            mF.mesh = airspaceMesh;
            gO.transform.position = disp;
            gO.layer = 8;

            Zone = gO;

            #endregion
        }

        /// <summary>
        /// Extracted from Mapbox.Unity.MeshGeneration.Modifiers.PolygonMeshModifier.
        /// </summary>
        private bool IsClockwise(List<Vector3> vertices)
        {
            double sum = 0.0;
            var _counter = vertices.Count;
            for (int i = 0; i < _counter; i++)
            {
                var _v1 = vertices[i];
                var _v2 = vertices[(i + 1) % _counter];
                sum += (_v2.x - _v1.x) * (_v2.z + _v1.z);
            }

            return sum > 0.0;
        }
    }
}
