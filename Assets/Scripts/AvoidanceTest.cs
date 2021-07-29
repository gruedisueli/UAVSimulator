using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// @Eunu is this obsolete?
/// </summary>
public class AvoidanceTest : MonoBehaviour
{
    // Start is called before the first frame update
    const int angleIncrement = 5;

    public Vector3 destination;
    public float radius;
    public List<Vector3> wayPoints;
    public Queue<Vector3> wayPointsQueue;
    public Vector3 currentTargetPosition;
    public float currentSpeed = 100.0f;

    private float arrival_threshold = 1.0f;
    private string state;
    void Start()
    {
        state = "idle";
        wayPoints = FindPath(transform.position, destination, 5);
        wayPointsQueue = new Queue<Vector3>();
        foreach (Vector3 v in wayPoints)
        {
            wayPointsQueue.Enqueue(v);
        }
        currentTargetPosition = wayPointsQueue.Dequeue();

        //Debug
        if ( Mathf.Abs(currentTargetPosition.x) > 10000 || Mathf.Abs(currentTargetPosition.z) > 10000)
        {
            int a;
            a = 0;
        }
        //~Debug

    }

    // Update is called once per frame
    void Update()
    {
        MoveAlong();
    }
    List<Vector3> FindPath (Vector3 origin, Vector3 destination, int angleIncrement)
    {
        // For pathfinding, omit drone colliders
        int head = 0, tail = 0;
        List<Vector3> visited = new List<Vector3>();
        List<int> from = new List<int>();
        List<float> distance = new List<float>();
        visited.Add(origin);
        from.Add(-1);
        distance.Add(0.0f);
        tail++;
        
        while( Physics.Raycast(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination)) && head <= tail )
        {
            RaycastHit currentHitObject = Physics.RaycastAll(visited[head], destination - visited[head], Vector3.Distance(visited[head], destination))[0];
            Vector3 lastHit = currentHitObject.point;
            for (int i = angleIncrement; i <= 85; i += angleIncrement)
            {
                Vector3 currentVector = destination - visited[head];
                RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0,i,0)*(destination - visited[head]), Vector3.Distance(visited[head], destination));
                if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                {
                    Vector3 newWaypoint = RotateAround(lastHit, visited[head], (float)angleIncrement);
                    visited.Add(newWaypoint);
                    from.Add(head);
                    distance.Add(distance[head] + Vector3.Distance(visited[head], newWaypoint));
                    tail++;
                    break;
                }
                else
                {
                    lastHit = hit[0].point;
                }
            }

            // Do the same thing in the opposite direction
            lastHit = currentHitObject.point;
            for (int i = angleIncrement; i <= 85; i += angleIncrement)
            {
                Vector3 currentVector = destination - visited[head];
                RaycastHit[] hit = Physics.RaycastAll(visited[head], Quaternion.Euler(0, -i, 0) * (destination - visited[head]), Vector3.Distance(visited[head], destination));
                if (hit.Length == 0 || !hit[0].transform.Equals(currentHitObject.transform)) // If the ray does not hit anything or does not hit the first hitted object anymore
                {
                    Vector3 newWaypoint = RotateAround(lastHit, visited[head], -(float)angleIncrement);
                    visited.Add(newWaypoint);
                    from.Add(head);
                    distance.Add(distance[head] + Vector3.Distance(visited[head], newWaypoint));
                    tail++;
                    break;
                }
                else
                {
                    lastHit = hit[0].point;
                }
            }

            head++;
        }
        // Do the same thing in the opposite direction
        List<Vector3> path = Backtrack(visited, from, distance, head);
        path.Add(destination);
        return path;
    }

    List<Vector3> Backtrack ( List<Vector3> visited, List<int> from, List<float> distance, int head)
    {
        List<Vector3> result = new List<Vector3>();
        int pointer = head;
        do
        {
            result.Add(visited[pointer]);
            pointer = from[pointer];
        } while (from[pointer] != -1);
        result.Add(visited[pointer]);
        result.Reverse();
        return result;
    }
    Vector3 RotateAround ( Vector3 point, Vector3 pivot, float angle )
    {
        Vector3 newPoint = point - pivot;
        newPoint = Quaternion.Euler(0, angle, 0) * newPoint;
        newPoint = newPoint + pivot;
        return newPoint;
    }

    void MoveAlong()
    {
        if (Vector3.Distance(gameObject.transform.position, currentTargetPosition) < arrival_threshold)
        {
            if (wayPointsQueue.Count == 0)
            {
                state = "move_ready";
                return;
            }
            else
            {
                currentTargetPosition = wayPointsQueue.Dequeue();
                return;
            }
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(currentTargetPosition - transform.position, transform.up);
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, KMHtoMPS(currentSpeed) * Time.deltaTime);
        }
    }
    private float KMHtoMPS(float speed)
    {
        return (speed * 1000) / 3600;
    }
}
