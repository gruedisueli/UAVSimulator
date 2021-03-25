using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingNoise : MonoBehaviour
{
    // Start is called before the first frame update
    VehicleControlSystem vcs;
    string material_path = "Materials/";
    Material lowNoiseMaterial;
    Material midNoiseMaterial;
    Material highNoiseMaterial;
    Material noNoiseMaterial;
    List<GameObject> affectingVehicles;
    void Start()
    {
        lowNoiseMaterial = Resources.Load<Material>(material_path + "LowNoise");
        midNoiseMaterial = Resources.Load<Material>(material_path + "MidNoise");
        highNoiseMaterial = Resources.Load<Material>(material_path + "HighNoise");
        noNoiseMaterial = Resources.Load<Material>(material_path + "Building");
        affectingVehicles = new List<GameObject>();
        vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (vcs.noiseVisualization)
        {
            if (affectingVehicles.Count == 1)
            {
                gameObject.GetComponent<MeshRenderer>().material = lowNoiseMaterial;
            }
            else if (affectingVehicles.Count == 2)
            {
                gameObject.GetComponent<MeshRenderer>().material = midNoiseMaterial;
            }
            else if (affectingVehicles.Count >= 3)
            {
                gameObject.GetComponent<MeshRenderer>().material = highNoiseMaterial;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material = noNoiseMaterial;
            }
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = noNoiseMaterial;
        }

    }
    public void AddNoise(GameObject affectingVehicle)
    {
        if (!affectingVehicles.Contains(affectingVehicle)) affectingVehicles.Add(affectingVehicle);
    }
    public void DecreaseNoise(GameObject affectingVehicle)
    {
        if (affectingVehicles.Contains(affectingVehicle)) affectingVehicles.Remove(affectingVehicle);
    }
}
