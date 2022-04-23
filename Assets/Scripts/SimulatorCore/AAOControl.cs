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
        //only allow vehicles to exist as long as they are completing a trip. Destroy when done with polygon path.
        foreach(GameObject vehicle in vehiclesInOperation)
        {
            if ( vehicle.GetComponent<DroneBase>().State != "parked" )
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

    /// <summary>
    /// Adds a new drone to simulation.
    /// </summary>
    public void AddVehicle (GameObject g)
    {
        if(vehiclesInOperation == null) vehiclesInOperation = new List<GameObject>();
        vehiclesInOperation.Add(g);
    }

    /// <summary>
    /// Returns the number of vehicles in operation.
    /// </summary>
    public int GetVehicleCount()
    {
        return vehiclesInOperation.Count;
    }

}
