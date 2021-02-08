using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictionZone2 : MonoBehaviour
{
    private List<GameObject> restrictionZones;
    public Material restrictionZoneMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        restrictionZones = new List<GameObject>();
    
        // Temporary restriction zone - test
        var restrictionZone = AddRestrictionZone(-1117.0f, 121.92f, 555.0f, 400.0f, 200.92f);
        restrictionZones.Add(restrictionZone);
        var restrictionZone2 = AddRestrictionZone(3573.0f, 121.92f, 616.0f, 250.0f, 200.92f);
        restrictionZones.Add(restrictionZone2);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Center X, Y, Z is the center of a cylineder at the bottom
    public GameObject AddRestrictionZone(float centerX, float centerY, float centerZ, float radius, float height)
    {
        float translatedY = Translate(centerY, height);
        float translatedHeight = height / 2;

        GameObject restrictionZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        restrictionZone.transform.position = new Vector3(centerX, translatedY, centerZ);
        restrictionZone.transform.localScale = new Vector3(radius, translatedHeight, radius);
        restrictionZone.name = "Restriction Zone";
        restrictionZone.AddComponent<MeshRenderer>();
        restrictionZone.GetComponent<MeshRenderer>().material = restrictionZoneMaterial;

        return restrictionZone;
    }

    float Translate (float y, float h)
    {
        return (y + h / 2);
    }

    float Sgn(float v)
    {
        if (v < 0) return -1;
        else return 1;
    }

    List<Vector3> CircleLineIntersection(Vector3 point1, Vector3 point2, Vector3 center, float radius)
    {
        point1 = point1 - center;
        point2 = point2 - center;

        float dx = point2.x - point1.x;
        float dz = point2.z - point1.z;
        float dr = Mathf.Sqrt(dx * dx + dz * dz);
        float D = point1.x * point2.z - point2.x * point1.z;

        List<Vector3> result = new List<Vector3>();
        if (Mathf.Pow(radius, 2.0f) * Mathf.Pow(dr, 2.0f) - Mathf.Pow(D, 2.0f) > 0)
        {
            Vector3 v = new Vector3();
            v.x = (D * dz + Sgn(dz) * dx * Mathf.Sqrt(Mathf.Pow(radius, 2.0f) * Mathf.Pow(dr, 2.0f) - Mathf.Pow(D, 2.0f))) / Mathf.Pow(dr, 2.0f);
            v.z = (-D * dx + Mathf.Abs(dz) * Mathf.Sqrt(Mathf.Pow(radius, 2.0f) * Mathf.Pow(dr, 2.0f) - Mathf.Pow(D, 2.0f))) / Mathf.Pow(dr, 2.0f);
            result.Add(v + center);

            v.x = (D * dz - Sgn(dz) * dx * Mathf.Sqrt(Mathf.Pow(radius, 2.0f) * Mathf.Pow(dr, 2.0f) - Mathf.Pow(D, 2.0f))) / Mathf.Pow(dr, 2.0f);
            v.z = (-D * dx - Mathf.Abs(dz) * Mathf.Sqrt(Mathf.Pow(radius, 2.0f) * Mathf.Pow(dr, 2.0f) - Mathf.Pow(D, 2.0f))) / Mathf.Pow(dr, 2.0f);
            result.Add(v + center);
        }
        return result;
    }

    // If List<Vector3> is empty - no restriction zone is crossed
    // If List<Vector3> > 0 - restriction zone is crossed and needs to go around
    List<Vector3> GoAroundRestrictionZone(Vector3 point1, Vector3 point2, float elevation)
    {
        float restrictionZoneBottom = 0.0f, restrictionZoneTop = 0.0f;
        bool withinHeight = false;

        List<Vector3> avoidingRoutes = new List<Vector3>();

        foreach (GameObject rz in restrictionZones)
        {
            withinHeight = false;
            restrictionZoneBottom = rz.transform.position.y - (rz.transform.localScale.y / 2);
            restrictionZoneTop = rz.transform.position.y + (rz.transform.localScale.y / 2);

            if (elevation >= restrictionZoneBottom && elevation <= restrictionZoneTop)
            {
                withinHeight = true;
            }
            else continue;

            var intersection = CircleLineIntersection(point1, point2, rz.transform.position, rz.transform.localScale.x);
            if (intersection.Count > 0)
            {
                Vector3 int1 = new Vector3();
                Vector3 int2 = new Vector3();
                if (Vector3.Distance(point1, intersection[0]) < Vector3.Distance(point1, intersection[1]))
                {
                    int1 = intersection[0];
                    int2 = intersection[1];
                }
                else
                {
                    int1 = intersection[1];
                    int2 = intersection[0];
                }
                int count = Mathf.RoundToInt(Vector3.Angle(int1 - rz.transform.position, int2 - rz.transform.position) / 30.0f);
                Vector3 vector = int1 - rz.transform.position;
                //avoidingRoutes.Add(int1);
                for (int i = 0; i < count; i++)
                {
                    vector = Quaternion.AngleAxis(30.0f, rz.transform.up) * vector;
                    var result = vector + rz.transform.position;
                    result.y = elevation;
                    avoidingRoutes.Add(result);
                }
            }
            else continue;
        }

        // Sort avoiding routes - distance from point1
        if (avoidingRoutes.Count == 0) return avoidingRoutes;
        else
        {
            Vector3 temp = new Vector3();
            for (int i = 0; i < avoidingRoutes.Count; i++)
            {
                for (int j = i + 1; j < avoidingRoutes.Count; j++)
                {
                    if (Vector3.Distance(point1, avoidingRoutes[i]) > Vector3.Distance(point1, avoidingRoutes[j]))
                    {
                        temp = avoidingRoutes[i];
                        avoidingRoutes[i] = avoidingRoutes[j];
                        avoidingRoutes[j] = temp;
                    }
                }

            }
        }
        string debug = "Intermediate Points: ";
        foreach ( var p in avoidingRoutes)
        {
            debug += p.ToString();
            debug += " - ";
        }
        Debug.Log(debug);
        return avoidingRoutes;
    }

    public Queue<Vector3> AvoidRestrictionZones(Vector3 point1, Vector3 point2, GameObject vehicle)
    {
        Queue<Vector3> avoidingRoutes = new Queue<Vector3>();
        Vehicle v = vehicle.GetComponent<Vehicle>();
        var result = GoAroundRestrictionZone(point1, point2, v.elevation);
        
        // Debug
        if (result.Count > 0)
        {
            int p = 0;
            p = 1;
        }
        // ~Debug
        //avoidingRoutes.Enqueue(point1);
        foreach (Vector3 p in result)
        {
            avoidingRoutes.Enqueue(p);
        }
        avoidingRoutes.Enqueue(point2);
        return avoidingRoutes;
    }
}
