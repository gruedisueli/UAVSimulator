using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleNoise : MonoBehaviour
{
    //SphereCollider col;

    DroneBase vehicleInfo;
    float noise;
    float radius;
    List<Collider> affected_buildings;
    GameObject noiseShpere;
    // Start is called before the first frame update
    void Start()
    {


        //noiseShpere = gameObject.transform.GetChild(1).gameObject;
        vehicleInfo = gameObject.GetComponent<DroneBase>();
        

        radius = 0.0f;
        affected_buildings = new List<Collider>();
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

    void CheckNoise()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, radius / 2);
        List<Collider> hitColliders_list = new List<Collider>(hitColliders);
        
        foreach(var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == "Building")
            {
                hitCollider.SendMessage("AddNoise", gameObject);
                if (!affected_buildings.Contains(hitCollider)) affected_buildings.Add(hitCollider);
            }
        }

        List<Collider> affected_building_copy = new List<Collider>(affected_buildings);
        foreach(var affected_building in affected_buildings)
        {
            if (!hitColliders_list.Contains(affected_building))
            {
                affected_building.SendMessage("DecreaseNoise", gameObject);
                affected_building_copy.Remove(affected_building);
            }
        }
        affected_buildings = affected_building_copy;
    }

}
