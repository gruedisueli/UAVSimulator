using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Environment;
using UnityEngine;

/// <summary>
/// Gets attached to a single drone and monitors this drone's noise effects on other simulation elements. @Eunu, correct?
/// </summary>
public class VehicleNoise : MonoBehaviour
{
    float noise;
    public float Radius { get; private set; }
    List<Collider> affected_buildings;
    VehicleControlSystem vcs;
    // Start is called before the first frame update
    void Awake()
    {
        affected_buildings = new List<Collider>();
    }

    public void Init(VehicleControlSystem controlSys)
    {
        vcs = controlSys;
        noise = 0;
        Radius = GetNoiseRadius();
    }

    public void SetSoundLevel(float soundDb)
    {
        noise = soundDb;
        Radius = GetNoiseRadius();
    }

    // Update is called once per frame
    void Update()
    {
        //noise = vehicleInfo.SoundAtSourceDb;
        //radius = vehicleInfo.CurrentSpeed;
        //noiseShpere.transform.localScale = new Vector3(radius,radius,radius);

        //col.radius = radius;
        CheckNoise();
    }

    /// <summary>
    /// Updates noise info, adding/removing elements that are affected to/frome our list field.
    /// </summary>
    void CheckNoise()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, Radius);
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

        for (int i =  affected_buildings.Count - 1; i >= 0; i--)
        {
            if (affected_buildings[i] == null)
            {
                affected_buildings.RemoveAt(i);
            }
            else if (!hitColliders_list.Contains(affected_buildings[i]))
            {
                var n = affected_buildings[i].gameObject.GetComponent<BuildingNoise>()?.ID;
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
                affected_buildings.RemoveAt(i);
            }
        }
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

    /// <summary>
    /// Noise radius is defined by distance from source at which sound level is equal to acceptable threshold, as defined by simulation settings
    /// For formulas, source was https://www.omnicalculator.com/physics/distance-attenuation
    /// </summary>
    private float GetNoiseRadius()
    {
        //SPL₂ = SPL₁ - 20 * log (R₂ / R₁)
        //where: SPL₁ = noise at source
        //SPL₂ = acceptable threshold
        //R₁ = 1 (dist at source)
        //R₂ = unknown radius where we hit acceptable threshold
        //rearrange formula for unknowns:
        //SPL₂ = SPL₁ - 20 * log (R₂)
        //SPL₂ - SPL₁ = -20 * log (R₂)
        //(SPL₂ - SPL₁)/(-20) = log (R₂)
        //10^((SPL₂ - SPL₁)/(-20)) = R₂
        return (float) Math.Pow(10, ((double) EnvironManager.Instance.Environ.SimSettings.AcceptableNoiseThreshold_Decibels - (double) noise) / (-20));
    }

}
