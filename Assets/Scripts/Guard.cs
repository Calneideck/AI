using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    private Vector3[] path;
    private int pathIndex;
    private bool requestingPath = true;

    public float speed = 4;

    void Start()
    {
        Pathfinding.instance.RequestPath(gameObject, transform.position);
    }

    void Update()
    {
        if (requestingPath)
            return;

        Vector3 target = path[pathIndex];
        target.y = 0.6f;
        transform.Translate((target - transform.position).normalized * speed * Time.deltaTime, Space.World);
        transform.LookAt(target);

        if (Vector3.Distance(transform.position, target) < 0.2f)
        {
            pathIndex++;
            if (pathIndex == path.Length)
            {
                Pathfinding.instance.RequestPath(gameObject, transform.position);
                requestingPath = true;
            }
        }
    }

    public void PathReceived(Vector3[] path)
    {
        this.path = path;
        pathIndex = 0;
        requestingPath = false;

        if (path.Length == 0)
        {
            Pathfinding.instance.RequestPath(gameObject, transform.position);
            requestingPath = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.red;
            for (int i = 1; i < path.Length; i++)
                Gizmos.DrawLine(path[i - 1], path[i]);
        }
    }
}
