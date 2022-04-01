using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdMovement : MonoBehaviour
{
    public Transform waypointParent = null;
    public Vector3 nextWaypoint = Vector3.zero;
    public float movementSpeed = 1.5f;
    public float rotationSpeed = 3f;
    public float maxdeviationInHeight = 0.05f;
    public float maxdeviationInArea = 0.3f;

    List<Vector3> waypoints = new List<Vector3>();
    int waypointNumber = 0;
    Vector3 randomDeviation = Vector3.zero;
    Animator anim = null;
    float distanceBetweenWaypoints = 1;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (waypointParent == null || anim == null)
            Debug.Log("Reference missing in BirdMovement.cs");

        for(int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints.Add(waypointParent.GetChild(i).position + transform.position);
        }
        
        nextWaypoint = waypoints[0];
    }

    void Update()
    {
        Move();
        Rotate();
    }
    
    void Move()
    {
        float distance_to_waypoint = Vector3.Distance(transform.position, nextWaypoint);

        if (distance_to_waypoint < 0.02f)
        {
            randomDeviation.x = Random.Range(-maxdeviationInArea, maxdeviationInArea);
            randomDeviation.y = Random.Range(-maxdeviationInHeight, maxdeviationInHeight);
            randomDeviation.z = Random.Range(-maxdeviationInArea, maxdeviationInArea);

            waypointNumber++;
            if (waypointNumber >= waypointParent.childCount)
                waypointNumber = 0;

            Vector3 previous_waypoint = nextWaypoint;
            nextWaypoint = waypoints[waypointNumber] + randomDeviation;

            distanceBetweenWaypoints = Vector3.Distance(previous_waypoint, nextWaypoint);

            if (nextWaypoint.y >= transform.position.y - 0.1f)
                anim.enabled = true;
            else
                anim.enabled = false;
        }
        else
        {
            float speed_change;

            if (nextWaypoint.y >= transform.position.y)
                speed_change = 1 - Mathf.Abs(distance_to_waypoint - distanceBetweenWaypoints / 2);
            else
                speed_change = 1 - Mathf.Abs(distance_to_waypoint - distanceBetweenWaypoints / 2) * 0.5f;
            if (speed_change < 0.3f)
                speed_change = 0.3f;

            // calculate distance to move
            float step = movementSpeed * Time.deltaTime * speed_change;
            transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);
        }
    }

    void Rotate()
    {
        // Determine which direction to rotate towards
        Vector3 target_direction = nextWaypoint - transform.position;

        // The step size is equal to speed times frame time.
        float step = rotationSpeed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 new_direction = Vector3.RotateTowards(transform.forward, target_direction, step, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, new_direction, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(new_direction);
    }
}
