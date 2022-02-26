using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets attached to a single drone and monitors this drone's noise effects on other simulation elements. @Eunu, correct?
/// </summary>
public class VehicleNoise : MonoBehaviour
{
    //SphereCollider col;

    DroneBase vehicleInfo;
    float noise;
    float radius;
    List<Collider> affected_buildings;
    GameObject noiseShpere;
    VehicleControlSystem vcs;
    // Start is called before the first frame update
    void Start()
    {


        //noiseShpere = gameObject.transform.GetChild(1).gameObject;
        vehicleInfo = gameObject.GetComponent<DroneBase>();
        

        radius = 0.0f;
        affected_buildings = new List<Collider>();
    }

    public void Init(VehicleControlSystem controlSys)
    {
        vcs = controlSys;
    }

    // Update is called once per frame
    void Update()
    {
        noise = vehicleInfo.GetNoise();
        radius = vehicleInfo.currentSpeed;
        //noiseShpere.transform.localScale = new Vector3(radius,radius,radius);

        //col.radius = radius;
        CheckNoise();
    }

    /// <summary>
    /// Updates noise info, adding/removing elements that are affected to/frome our list field.
    /// </summary>
    void CheckNoise()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, radius / 2);
        List<Collider> hitColliders_list = new List<Collider>(hitColliders);
        
        foreach(var hitCollider in hitColliders)
        {
            
            if (hitCollider.gameObject.tag == "Building")
            {
                var n = hitCollider.gameObject.GetComponent<BuildingNoise>()?.ID;
                if (n == null) continue;
                if (vcs.BuildingNoiseElements.ContainsKey(n))
                {
                    vcs.BuildingNoiseElements[n].AddNoise(gameObject);
                }
                else
                {
                    Debug.LogError("Building not found in noise element dictionary");
                    continue;
                }

                if (!affected_buildings.Contains(hitCollider))
                {
                    var noise = hitCollider.GetComponent<BuildingNoise>();
                    if (noise != null)
                    {
                        noise.OnDestroyed += BuildingDestroyedAction;
                    }
                    affected_buildings.Add(hitCollider);
                }
            }
        }

        List<Collider> affected_building_copy = new List<Collider>(affected_buildings);
        foreach(var affected_building in affected_buildings)
        {
            if (!hitColliders_list.Contains(affected_building))
            {
                var n = affected_building.gameObject.GetComponent<BuildingNoise>()?.ID;
                if (n == null) continue;
                if (vcs.BuildingNoiseElements.ContainsKey(n))
                {
                    vcs.BuildingNoiseElements[n].DecreaseNoise(gameObject);
                }
                else
                {
                    Debug.LogError("Building not found in noise element dictionary");
                    continue;
                }
                affected_building_copy.Remove(affected_building);
            }
        }
        affected_buildings = affected_building_copy;
    }

    /// <summary>
    /// Called when a building gameobject in affected buildings list is destroyed
    /// </summary>
    private void BuildingDestroyedAction(object s, System.EventArgs args)
    {
        if (s is BuildingNoise noise)
        {
            var c = noise.GetComponent<Collider>();
            if (c != null)
            {
                affected_buildings.Remove(c);
            }
        }
    }

    void OnDestroy()
    {
        foreach (var b in affected_buildings)
        {
            var n = b?.GetComponent<BuildingNoise>();
            if (n != null)
            {
                n.OnDestroyed -= BuildingDestroyedAction;
            }
        }
    }

}
