using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;
using Assets.Scripts.SimulatorCore;
using UnityEditor;

/// <summary>
/// Gets atached to every building in simulation that want to monitor sound on.
/// </summary>
public class BuildingNoise : MonoBehaviour
{
    public EventHandler<EventArgs> OnDestroyed;

    // Start is called before the first frame update
    VehicleControlSystem vcs;
    Material lowNoiseMaterial;
    Material midNoiseMaterial;
    Material highNoiseMaterial;
    Material noNoiseMaterial;
    List<GameObject> affectingVehicles = new List<GameObject>();
    SimulationAnalyzer simulationAnalyzer;
    GameObject building;
    public string ID { get; private set; }

    private bool _registered = false;

    private void Awake()
    {
        vcs = EnvironManager.Instance.VCS;
        if (vcs == null)
        {
            Debug.LogError("Could not locate vehicle control system");
            return;
        }

        lowNoiseMaterial = vcs._lowNoiseMaterial;
        midNoiseMaterial = vcs._midNoiseMaterial;
        highNoiseMaterial = vcs._highNoiseMaterial;
        noNoiseMaterial = vcs._noNoiseMaterial;
        simulationAnalyzer = vcs._simulationAnalyzer;
        building = gameObject;
        gameObject.layer = 9;
        gameObject.tag = "Building";

    }

    /// <summary>
    /// Increases reported noise level on building.
    /// Noise level is affected by number of nearby drones, but not proximity to drones. @Eunu, correct?
    /// </summary>
    public void AddNoise(GameObject affectingVehicle)
    {
        if (!affectingVehicles.Contains(affectingVehicle))
        {
            if (affectingVehicles.Count == 0)
            {
                simulationAnalyzer.SendMessage("AddLowNoiseBuilding", building);
            }
            else if (affectingVehicles.Count == 1)
            {
                simulationAnalyzer.SendMessage("AddMediumNoiseBuilding", building);
                simulationAnalyzer.SendMessage("RemoveLowNoiseBuilding", building);
            }
            else if (affectingVehicles.Count == 2)
            {
                simulationAnalyzer.SendMessage("AddHighNoiseBuilding", building);
                simulationAnalyzer.SendMessage("RemoveMediumNoiseBuilding", building);
            }

            affectingVehicles.Add(affectingVehicle);
            UpdateVisual();
        }
    }

    /// <summary>
    /// Reduces reported noise level on building.
    /// </summary>
    public void DecreaseNoise(GameObject affectingVehicle)
    {
        if (affectingVehicles.Contains(affectingVehicle))
        {
            if (affectingVehicles.Count == 3)
            {
                simulationAnalyzer.SendMessage("RemoveHighNoiseBuilding", building);
                simulationAnalyzer.SendMessage("AddMediumNoiseBuilding", building);
            }
            else if (affectingVehicles.Count == 2)
            {
                simulationAnalyzer.SendMessage("RemoveMediumNoiseBuilding", building);
                simulationAnalyzer.SendMessage("AddLowNoiseBuilding", building);
            }
            else if (affectingVehicles.Count == 1)
            {
                simulationAnalyzer.SendMessage("RemoveLowNoiseBuilding", building);
            }

            affectingVehicles.Remove(affectingVehicle);
            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        if (vcs.noiseVisualization)
        {
            if (affectingVehicles.Count == 1)
            {
                building.GetComponent<MeshRenderer>().material = lowNoiseMaterial;
            }
            else if (affectingVehicles.Count == 2)
            {
                building.GetComponent<MeshRenderer>().material = midNoiseMaterial;
            }
            else if (affectingVehicles.Count >= 3)
            {
                building.GetComponent<MeshRenderer>().material = highNoiseMaterial;
            }
            else
            {
                building.GetComponent<MeshRenderer>().material = noNoiseMaterial;
            }
        }
        else
        {
            building.GetComponent<MeshRenderer>().material = noNoiseMaterial;
        }
    }

    void OnEnable()
    {
        if (!_registered)
        {
            ID = GUID.Generate().ToString();
            vcs.RegisterNoiseComponent(this);
            _registered = true;
        }
    }

    void OnDestroy()
    {
        if (_registered)
        {
            vcs.DeRegisterNoiseComponent(this);
        }
        OnDestroyed?.Invoke(this, new EventArgs());
    }

}
