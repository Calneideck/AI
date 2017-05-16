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
        Pathfinding.instance.RequestPath(gameObject, transform.position, new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)));
    }

    void Update()
    {
        if (requestingPath)
            return;

        Vector3 target = path[pathIndex];
        transform.Translate((target - transform.position).normalized * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.2f)
        {
            pathIndex++;
            if (pathIndex == path.Length)
            {
                Pathfinding.instance.RequestPath(gameObject, transform.position, new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)));
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
            Pathfinding.instance.RequestPath(gameObject, transform.position, new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)));
            requestingPath = true;
        }
    }
}
