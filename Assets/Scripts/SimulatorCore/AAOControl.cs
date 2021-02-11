using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAOControl : MonoBehaviour
{
    // Start is called before the first frame update
    private List<GameObject> vehiclesInOperation;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool isFinished = false;
        foreach(GameObject vehicle in vehiclesInOperation)
        {
            if ( vehicle.GetComponent<Vehicle>().state != "parked" )
            {
                isFinished = false;
                break;
            }
            else
            {
                isFinished = true;
            }
        }
        if (isFinished)
        {
            Destroy(gameObject);
        }

    }
    public void AddVehicle (GameObject g)
    {
        if(vehiclesInOperation == null) vehiclesInOperation = new List<GameObject>();
        vehiclesInOperation.Add(g);
    }
    public int GetVehicleCount()
    {
        return vehiclesInOperation.Count;
    }

}
