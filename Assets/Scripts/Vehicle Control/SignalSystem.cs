using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Vehicle_Control;

public class SignalSystem : MonoBehaviour
{
    // Start is called before the first frame update
    
    private Dictionary<GameObject, VehicleControlSignal> registeredSignals;
    void Start()
    {
        registeredSignals = new Dictionary<GameObject, VehicleControlSignal>();
    }

    // Update is called once per frame
    void Update()
    {
                
    }
    public void RegisterSignal (GameObject target, VehicleControlSignal signal)
    {
        // adds only when there is no entry with the key "target"
        if(!registeredSignals.ContainsKey(target))  registeredSignals.Add(target, signal);

    }
    public VehicleControlSignal UnregisterSignal(GameObject target)
    {
        if (!registeredSignals.ContainsKey(target)) return null;
        else
        {
            VehicleControlSignal vcs = registeredSignals[target];
            registeredSignals.Remove(target);
            return vcs;
        }
    }
    public bool hasSignaltoReceive (GameObject target)
    {
        if (registeredSignals.ContainsKey(target)) return true;
        else return false;
    }
}
