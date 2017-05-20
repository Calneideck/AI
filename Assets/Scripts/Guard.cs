using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guard : MonoBehaviour
{
    private enum State { PATROLLING, ENGAGING, RELOADING, ALERTED };
    private State state = State.PATROLLING;

    private Vector3[] path;
    private int pathIndex;
    private bool requestingPath = true;
    private Player player;

    public float detection = 0;

    public float speed = 4;
    public float minSightRange;
    public float maxSightRange;
    public Image detectionImage;
    public LayerMask wallMask;

    void Start()
    {
        Pathfinding.instance.RequestPath(gameObject, transform.position);
        player = GameObject.Find("Player").GetComponent<Player>();
        minSightRange *= minSightRange;
        maxSightRange *= maxSightRange;
    }

    void Update()
    {
        switch (state)
        {
            case State.PATROLLING:
                Move();
                break;
            case State.ENGAGING:

                break;
            case State.ALERTED:

                break;
            case State.RELOADING:


                break;
        }

        DetectPlayer();
        detectionImage.transform.parent.rotation = Quaternion.Euler(Vector3.right * 90);
    }

    void DetectPlayer()
    {
        // Check if has line of sight
        RaycastHit wallHit;
        if (Physics.Raycast(transform.position, player.transform.position - transform.position, out wallHit, 100, wallMask))
            if (wallHit.collider != null)
            {
                detection = 1 - Mathf.InverseLerp(minSightRange, maxSightRange, (player.transform.position - transform.position).sqrMagnitude);

                float los = 1;
                if (Vector3.Angle(player.transform.position - transform.position, transform.forward) < 60)
                    los = 1.4f;

                detection *= los;
            }

        detectionImage.fillAmount = detection;
    }

    void Move()
    {
        if (requestingPath)
            return;

        Vector3 target = path[pathIndex];
        target.y = 0.3f;
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
        if (DebugHelper.paths && path != null)
        {
            Gizmos.color = Color.red;
            for (int i = 1; i < path.Length; i++)
                Gizmos.DrawLine(path[i - 1], path[i]);
        }

        if (DebugHelper.sightPaths)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? Mathf.Sqrt(minSightRange) : minSightRange);
            Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? Mathf.Sqrt(maxSightRange) : maxSightRange);
        }
    }
}
