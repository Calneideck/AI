using UnityEngine;
using System.Collections;

public class Patrol : MonoBehaviour
{
    public float searchTime = 3;
    public Guard guard;

    public BNode.ResultState FollowPath()
    {
        if (guard.Path != null && guard.PathIndex < guard.Path.Length)
        {
            Vector3 target = guard.Path[guard.PathIndex];
            target.y = 0.3f;
            transform.Translate((target - transform.position).normalized * guard.speed * Time.deltaTime, Space.World);
            transform.LookAt(target);

            if (Vector3.Distance(transform.position, target) < 0.2f)
            {
                guard.PathIndex++;
                if (guard.PathIndex == guard.Path.Length)
                    return BNode.ResultState.SUCCESS;
            }
        }

        return BNode.ResultState.RUNNING;
    }

    public void OnFollowPath()
    {
        Pathfinding.instance.RequestPath(gameObject);
    }

    public BNode.ResultState Search()
    {
        float pct = Mathf.Clamp01((Time.time - guard.StartTaskTime) / searchTime);
        transform.rotation = Quaternion.Euler(0, guard.StartRotation + 360 * pct, 0);
        if (pct == 1)
            return BNode.ResultState.SUCCESS;

        return BNode.ResultState.RUNNING;
    }
}
