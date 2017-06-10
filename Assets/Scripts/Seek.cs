using UnityEngine;
using System.Collections;

public class Seek : MonoBehaviour
{
    public Guard guard;

    public BNode.ResultState Follow()
    {
        if (guard.AlertedGuard == null)
            return BNode.ResultState.FAILURE;

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

    public void OnFollow()
    {
        if (guard.AlertedGuard != null)
            Pathfinding.instance.RequestPath(gameObject, guard.AlertedGuard.transform.position);
    }

    public void OnCheckLocation()
    {
        if (guard.InvestigateLocation != Vector3.zero)
            Pathfinding.instance.RequestPath(gameObject, guard.InvestigateLocation);
    }
}
