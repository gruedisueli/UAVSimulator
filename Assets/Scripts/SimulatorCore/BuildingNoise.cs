using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.SimulatorCore;

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
    SimulationAnalyzer simulationAnalyzer;

    void Start()
    {
        lowNoiseMaterial = Resources.Load<Material>(material_path + "LowNoise");
        midNoiseMaterial = Resources.Load<Material>(material_path + "MidNoise");
        highNoiseMaterial = Resources.Load<Material>(material_path + "HighNoise");
        noNoiseMaterial = Resources.Load<Material>(material_path + "Building");
        affectingVehicles = new List<GameObject>();
        vcs = GameObject.Find("SimulationCore").GetComponent<VehicleControlSystem>();
        simulationAnalyzer = GameObject.Find("SimulationCore").GetComponent<SimulationAnalyzer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!vcs.playing) return;
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
        if (!affectingVehicles.Contains(affectingVehicle))
        {
            if (affectingVehicles.Count == 0)
            {
                simulationAnalyzer.SendMessage("AddLowNoiseBuilding", this.gameObject);
            }
            else if (affectingVehicles.Count == 1)
            {
                simulationAnalyzer.SendMessage("AddMediumNoiseBuilding", this.gameObject);
                simulationAnalyzer.SendMessage("RemoveLowNoiseBuilding", this.gameObject);
            }
            else if (affectingVehicles.Count == 2)
            {
                simulationAnalyzer.SendMessage("AddHighNoiseBuilding", this.gameObject);
                simulationAnalyzer.SendMessage("RemoveMediumNoiseBuilding", this.gameObject);
            }
            affectingVehicles.Add(affectingVehicle);
        }
    }
    public void DecreaseNoise(GameObject affectingVehicle)
    {
        if (affectingVehicles.Contains(affectingVehicle))
        {
            if (affectingVehicles.Count == 3)
            {
                simulationAnalyzer.SendMessage("RemoveHighNoiseBuilding", this.gameObject);
                simulationAnalyzer.SendMessage("AddMediumNoiseBuilding", this.gameObject);
            }
            else if (affectingVehicles.Count == 2)
            {
                simulationAnalyzer.SendMessage("RemoveMediumNoiseBuilding", this.gameObject);
                simulationAnalyzer.SendMessage("AddLowNoiseBuilding", this.gameObject);
            }
            else if (affectingVehicles.Count == 1)
            {
                simulationAnalyzer.SendMessage("RemoveLowNoiseBuilding", this.gameObject);
            }
            affectingVehicles.Remove(affectingVehicle);
        }
    }
}
