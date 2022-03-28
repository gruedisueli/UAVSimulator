using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DataStructure
{

    [Serializable]
    class VehicleSpec
    {
        public string type;
        public int capacity;
        public float maxSpeed;
        public float landingSpeed;
        public float takeoffSpeed;
        public float yawSpeed;
        public float range;
        public float size;
        public List<float> emission;
        public List<float> noise;
    }

    [Serializable]
    #region Simulation Parameters
    public class SimulationParam
    {
        public float maxSpeed;
        public float verticalSeparation;
        public float horizontalSeparation;
        public float maxVehicleCount;
        public float takeoffSpeed;
        public float landingSpeed;
        public float maxYawSpeed;
        public float callGenerationInterval;
        public float maxInFlightVehicles;
        public float inCorridorSeparation;
        public float lowAltitudeBoundary;
        public string strategicDeconfliction;
        public string tacticalDeconfliction;

    }
    #endregion

    /// <summary>
    /// A basic polygon
    /// </summary>
    public class Polygon
    {
        public Vector3[] polygon;
        public Bounds bBox;

        /// <summary>
        /// Build a random polygon of set number of vertices, of arbitrary width/height
        /// </summary>
        public Polygon(int vertexCount)
        {
            polygon = new Vector3[vertexCount];
            float width = UnityEngine.Random.Range(100, 1000);
            float height = UnityEngine.Random.Range(100, 1000);
            
            polygon = MakeRandomPolygon(vertexCount, width, height);
        }

        /// <summary>
        /// Build a random polygon of set number of vertices, of specified width/height
        /// </summary>
        public Polygon(int vertexCount, float width, float height)
        {
            Vector3[] polygon = new Vector3[vertexCount];
            polygon = MakeRandomPolygon(vertexCount, width, height);
        }

        /// <summary>
        /// Building polygon using explicitly defined vertices.
        /// </summary>
        public Polygon(Vector3[] vertecies)
        {
            polygon = vertecies;
        }

        /// <summary>
        /// Makes random polygon vertex list with specified number of vertices, width, and height.
        /// </summary>
        private Vector3[] MakeRandomPolygon(int num_vertices, float width, float height)
        {
            float minX = Mathf.Infinity, minZ = Mathf.Infinity, maxX = Mathf.NegativeInfinity, maxZ = Mathf.NegativeInfinity;

            // Pick random radii.
            double[] radii = new double[num_vertices];
            const float min_radius = 0.5f;
            const float max_radius = 1.0f;
            for (int i = 0; i < num_vertices; i++)
            {
                radii[i] = UnityEngine.Random.Range(min_radius, max_radius);
            }

            // Pick random angle weights.
            double[] angle_weights = new double[num_vertices];
            const float min_weight = 1.0f;
            const float max_weight = 10.0f;
            double total_weight = 0;
            for (int i = 0; i < num_vertices; i++)
            {
                angle_weights[i] = UnityEngine.Random.Range(min_weight, max_weight);
                total_weight += angle_weights[i];
            }

            // Convert the weights into fractions of 2 * Pi radians.
            double[] angles = new double[num_vertices];
            double to_radians = 2 * Math.PI / total_weight;
            for (int i = 0; i < num_vertices; i++)
            {
                angles[i] = angle_weights[i] * to_radians;
            }

            // Calculate the points' locations.
            Vector3[] points = new Vector3[num_vertices];
            float rx = width / 2f;
            float ry = height / 2f;
            float cx = 0.0f;
            float cy = 0.0f;
            double theta = 0;
            for (int i = 0; i < num_vertices; i++)
            {
                points[i] = new Vector3(
                    cx + (int)(rx * radii[i] * Math.Cos(theta)),
                    0.0f,
                    cy + (int)(ry * radii[i] * Math.Sin(theta)));
                theta += angles[i];

                if (minX > points[i].x) minX = points[i].x;
                if (minZ > points[i].z) minZ = points[i].z;
                if (maxX < points[i].x) maxX = points[i].x;
                if (maxZ < points[i].z) maxZ = points[i].z;
            }

            bBox.SetMinMax(new Vector3(minX, 0, minZ), new Vector3(maxX, 0, maxZ));

            // Return the points.
            return points;
        }

        /// <summary>
        /// Converts polygon 3d vertices into 2d XZ ones., where X = unity X and Y = unity Z in the Vector2 struct.
        /// </summary>
        private Vector2[] Vec3toVec2()
        {
            List<Vector2> vec2 = new List<Vector2>();
            foreach(Vector3 v in polygon.ToList())
            {
                Vector2 p = new Vector2();
                p.x = v.x;
                p.y = v.z;
                vec2.Add(p);
            }
            return vec2.ToArray();
        }

        /// <summary>
        /// Moves all polygon vertices by same vector (moves the polygon)
        /// </summary>
        public void Move(Vector3 target)
        {
            for (int i = 0; i < polygon.Length; i++)
               polygon[i] += target;

            bBox.center += target;
            
        }

        /// <summary>
        /// Extrudes 2d polygon into a mesh.
        /// </summary>
        public Mesh CreateExtrusion(float height)
        {
            // convert polygon to triangles
            Vector2[] poly = Vec3toVec2();
            Triangulator triangulator = new Triangulator(poly);
            int[] tris = triangulator.Triangulate();
            Mesh m = new Mesh();
            Vector3[] vertices = new Vector3[poly.Length * 2];

            for (int i = 0; i < poly.Length; i++)
            {
                vertices[i].x = poly[i].x;
                vertices[i].z = poly[i].y;
                vertices[i].y = 0; // front vertex
                vertices[i + poly.Length].x = poly[i].x;
                vertices[i + poly.Length].z = poly[i].y;
                vertices[i + poly.Length].y = height;  // back vertex    
            }
            int[] triangles = new int[tris.Length * 2 + poly.Length * 6];
            int count_tris = 0;
            for (int i = 0; i < tris.Length; i += 3)
            {
                triangles[i] = tris[i];
                triangles[i + 1] = tris[i + 1];
                triangles[i + 2] = tris[i + 2];
            } // front vertices
            count_tris += tris.Length;
            for (int i = 0; i < tris.Length; i += 3)
            {
                triangles[count_tris + i] = tris[i + 2] + poly.Length;
                triangles[count_tris + i + 1] = tris[i + 1] + poly.Length;
                triangles[count_tris + i + 2] = tris[i] + poly.Length;
            } // back vertices
            count_tris += tris.Length;
            for (int i = 0; i < poly.Length; i++)
            {
                int n = (i + 1) % poly.Length;
                triangles[count_tris] = n;
                triangles[count_tris + 1] = i + poly.Length;
                triangles[count_tris + 2] = i;
                triangles[count_tris + 3] = n;
                triangles[count_tris + 4] = n + poly.Length;
                triangles[count_tris + 5] = i + poly.Length;
                count_tris += 6;
            }
            m.vertices = vertices;
            m.triangles = triangles;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.Optimize();
            return m;
        }

        /// <summary>
        /// Tests point for 2d inclusion in 2d polygon.
        /// </summary>
        public bool isPointInPolygon(Vector3 point)
        {
            Vector2[] polygonV2 = Vec3toVec2();
            Vector2 pt = new Vector2(point.x, point.z);

            int i, j = polygonV2.Length - 1;
            bool oddNodes = false;

            for (i = 0; i < polygonV2.Length; i++)
            {
                if ((polygonV2[i].y < pt.y && polygonV2[j].y >= pt.y
                || polygonV2[j].y < pt.y && polygonV2[i].y >= pt.y)
                && (polygonV2[i].x <= pt.x || polygonV2[j].x <= pt.x))
                {
                    oddNodes ^= (polygonV2[i].x + (pt.y - polygonV2[i].y) / (polygonV2[j].y - polygonV2[i].y) * (polygonV2[j].x - polygonV2[i].x) < pt.x);
                }
                j = i;
            }
            return oddNodes;
        }

        /// <summary>
        /// Generates random points within the extruded polygon.
        /// </summary>
        public List<Vector3> GeneratePointsinExtrusion(int count, float height)
        {
            List<Vector3> generatedPoints = new List<Vector3>();
            Vector3 v = new Vector3();
            for (int i = 0; i < count; i++)
            {
                do
                {
                    v.x = UnityEngine.Random.Range(bBox.min.x, bBox.max.x);
                    v.z = UnityEngine.Random.Range(bBox.min.z, bBox.max.z);
                    v.y = 0;
                } while (!isPointInPolygon(v));
                v.y = UnityEngine.Random.Range(height / 2, height);
                generatedPoints.Add(v);
            }
            return generatedPoints;
        }
    }

    /// <summary>
    /// @Eunu Comment.
    /// </summary>
    public class Triangulator
    {
        private List<Vector2> m_points = new List<Vector2>();

        /// <summary>
        /// Basic constructor, does not actually "triangulate" the set of points.
        /// </summary>
        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        /// <summary>
        /// @Eunu comment
        /// </summary>
        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        /// <summary>
        /// @Eunu Comment. What is meant by "Area" of a triangulation.
        /// </summary>
        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        /// <summary>
        /// @Eunu comment
        /// </summary>
        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// @Eunu comment
        /// </summary>
        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }

    /// <summary>
    /// A pathway for a corridor drone.
    /// </summary>
    [Serializable]
    public class Corridor
    {

        public delegate void OnCongestionLevelChangeDelegate(Corridor corridor, int congestionLevel);
        public event OnCongestionLevelChangeDelegate OnCongestionLevelChange;

        public GameObject origin;
        public GameObject destination;
        public float elevation;
        public Queue<Vector3> wayPoints;//@Eunu comment. What do waypoints mean within a single corridor? Why is it a Queue?
        public List<GameObject> dronesInCorridor;
        public float speedSum;//@Eunu comment
        public float maxSpeed;//max allowed speed in corridor? @Eunu comment?

        private int _congestionLevel;
        /// <summary>
        /// Get or set the congestion level for the corridor. On set to new value, invokes event.
        /// </summary>
        public int congestionLevel
        {
            get
            {
                return _congestionLevel;
            }
            set
            {
                if (_congestionLevel == value) return;
                else
                {
                    _congestionLevel = value;
                    if (OnCongestionLevelChange != null)
                    {
                        OnCongestionLevelChange(this, _congestionLevel);
                    }
                }
            }
        }
        public float averageSpeed
        {
            get
            {
                return speedSum / dronesInCorridor.Count;
            }
        }

        /// <summary>
        /// Compares two corridors for positional equivalency
        /// </summary>
        public bool Equals(Corridor p1, Corridor p2)
        {
            if (p1.origin.transform.position.Equals(p2.origin.transform.position) && p1.destination.transform.position.Equals(p2.destination.transform.position) && Mathf.Abs(p1.elevation - p2.elevation) < 0.001) return true;
            else return false;
        }

        /// <summary>
        /// Builds corridor between two points at specified elevation.
        /// </summary>
        public Corridor(GameObject org, GameObject dest, float elev)
        {
            origin = org;
            destination = dest;
            elevation = elev;
            dronesInCorridor = new List<GameObject>();
            speedSum = 0.0f;
            _congestionLevel = 0;
            maxSpeed = 200.0f;
        }

        /// <summary>
        /// @Eunu comment. Are we actually adding a drone? 
        /// </summary>
        public void AddDrone ( GameObject drone )
        {
            CorridorDrone cd = drone.GetComponent<CorridorDrone>();
            cd.OnInCorridorSpeedChange += SpeedChangeHandler;
            dronesInCorridor.Add(drone);
        }

        /// <summary>
        /// @Eunu comment. Are we actually removing a drone? 
        /// </summary>
        public void RemoveDrone(GameObject drone)
        {
            CorridorDrone cd = drone.GetComponent<CorridorDrone>();
            cd.OnInCorridorSpeedChange -= SpeedChangeHandler;
            dronesInCorridor.Remove(drone);
        }

        /// <summary>
        /// @Eunu comment.
        /// </summary>
        private void SpeedChangeHandler(float oldSpeed, float newSpeed)
        {
            speedSum -= oldSpeed;
            speedSum += newSpeed;
            if (averageSpeed / maxSpeed > 0.7f && averageSpeed / maxSpeed < 0.8f) congestionLevel = 1;
            else if (averageSpeed / maxSpeed > 0.6f && averageSpeed / maxSpeed < 0.7f) congestionLevel = 2;
            else if (averageSpeed / maxSpeed <= 0.6f && dronesInCorridor.Count > 0) congestionLevel = 3;
            else congestionLevel = 0;
        }


    }

    /// <summary>
    /// The network of all the corridors in the simulation. @Eunu comment.
    /// </summary>
    [Serializable]
    public class Network
    {
        public List<GameObject> vertices { get; set; }
        public List<Corridor> corridors { get; set; }
        public Dictionary<GameObject, List<Corridor>> outEdges { get; set; }
        public Dictionary<GameObject, List<Corridor>> inEdges { get; set; }


        public Network()
        {
            vertices = new List<GameObject>();
            corridors = new List<Corridor>();
            outEdges = new Dictionary<GameObject, List<Corridor>>();
            inEdges = new Dictionary<GameObject, List<Corridor>>();
        }
    }


}
