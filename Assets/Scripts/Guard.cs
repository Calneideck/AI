using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Guard : MonoBehaviour
{
    public enum State { PATROLLING, SEEKING, ENGAGING };
    private State state = State.PATROLLING;

    private Vector3[] path;
    private int pathIndex;
    private Player player;
    private float lastShootTime;
    private int ammo = 5;
    private BehaviourTree bTree;
    private float detection = 0;
    private float startTaskTime;
    private float startRotation;

    public float speed = 4;
    public float searchTime = 1;
    public float reloadTime = 1.8f;
    public float minSightRange = 2.5f;
    public float maxSightRange = 8;
    public float shootCooldown = 0.9f;
    public GameObject bulletPrefab;
    public Image detectionImage;
    public LayerMask wallMask;

    void Start()
    {
        bTree = GetComponent<BehaviourTree>();
        ObjectPooler.instance.Setup(bulletPrefab, 2);
        player = GameObject.Find("Player").GetComponent<Player>();
        minSightRange *= minSightRange;
        maxSightRange *= maxSightRange;
    }

    void Update()
    {
        switch (state)
        {
            case State.PATROLLING:
                bTree.Patrol.Update();
                DetectPlayer();
                break;
            case State.SEEKING:
                
                break;
            case State.ENGAGING:
                if (bTree.Engage.Update() == BNode.ResultState.SUCCESS)
                    state = State.PATROLLING;
                break;
        }
        
        detectionImage.transform.parent.rotation = Quaternion.Euler(Vector3.right * 90);
    }

    void DetectPlayer()
    {
        if (CanSeePlayer)
        {
            detection = 1 - Mathf.InverseLerp(minSightRange, maxSightRange, (player.transform.position - transform.position).sqrMagnitude);

            float los = 1;
            if (Vector3.Angle(player.transform.position - transform.position, transform.forward) < 60)
                los = 1.4f;

            detection *= los;
        }
        else
            detection = 0;

        if (detection >= 1)
        {
            Pathfinding.instance.RequestPath(gameObject, player.transform.position);
            state = State.ENGAGING;
        }

        detectionImage.fillAmount = detection;
    }

    #region PATROL
    public BNode.ResultState FollowPath()
    {
        if (path != null && pathIndex < path.Length)
        {
            Vector3 target = path[pathIndex];
            target.y = 0.3f;
            transform.Translate((target - transform.position).normalized * speed * Time.deltaTime, Space.World);
            transform.LookAt(target);

            if (Vector3.Distance(transform.position, target) < 0.2f)
            {
                pathIndex++;
                if (pathIndex == path.Length)
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
        float pct = Mathf.Clamp01((Time.time - startTaskTime) / searchTime);
        transform.rotation = Quaternion.Euler(0, startRotation + 360 * pct, 0);
        if (pct == 1)
            return BNode.ResultState.SUCCESS;

        return BNode.ResultState.RUNNING;
    }
    #endregion

    #region ENGAGE
    public BNode.ResultState Pursue()
    {
        if (Time.time - lastShootTime >= shootCooldown && CanSeePlayer)
        {
            GameObject bullet = ObjectPooler.instance.GetObject(bulletPrefab);
            bullet.transform.position = transform.position + transform.forward * 0.4f;
            Vector3 shootTarget = player.transform.position - transform.position;
            shootTarget.y = 0;
            bullet.GetComponent<Bullet>().Setup(shootTarget);
            lastShootTime = Time.time;
            if (--ammo == 0)
                return BNode.ResultState.FAILURE;
        }

        if (pathIndex < path.Length)
        {
            Vector3 moveTarget = path[pathIndex];
            // Move at 80% speed while pursuing
            transform.Translate((moveTarget - transform.position).normalized * speed * 0.8f * Time.deltaTime, Space.World);
            if (CanSeePlayer)
                transform.LookAt(player.transform);
            else
                transform.LookAt(moveTarget);

            if ((moveTarget - transform.position).sqrMagnitude < 0.04f)
            {
                pathIndex++;
                if (pathIndex == path.Length)
                    Pathfinding.instance.RequestPath(gameObject, player.transform.position);
            }
        }

        return BNode.ResultState.RUNNING;
    }

    public void OnPursue()
    {
        Pathfinding.instance.RequestPath(gameObject, player.transform.position);
    }

    public BNode.ResultState SeekCover()
    {
        return BNode.ResultState.RUNNING;
    }

    public void OnSeekCover()
    {
        while (true)
        {
            Vector3 coverPos = Random.insideUnitCircle * 4;
            coverPos = transform.position + (transform.position - player.transform.position).normalized * 3 + coverPos;

            if (Physics.Raycast(coverPos, player.transform.position - coverPos, Vector3.Distance(coverPos, player.transform.position), wallMask))
                if (Pathfinding.instance.NodeGrid.NodeFromWorldPoint(coverPos).Walkable)
                {
                    Pathfinding.instance.RequestPath(gameObject, coverPos);
                    break;
                }
        }
    }

    public BNode.ResultState Reload()
    {
        if (Time.time - startTaskTime >= reloadTime)
        {
            ammo = 5;
            // Must return failure in order to go back to pursuing
            return BNode.ResultState.FAILURE;
        }

        return BNode.ResultState.RUNNING;
    }
    #endregion

    public void PathReceived(Vector3[] path)
    {
        if (path.Length == 0)
            Pathfinding.instance.RequestPath(gameObject);
        else
        {
            this.path = path;
            pathIndex = 0;
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

    public void OnSearch()
    {
        startRotation = transform.rotation.eulerAngles.y;
        startTaskTime = Time.time;
    }

    bool CanSeePlayer
    {
        get { return !Physics.Raycast(transform.position, player.transform.position - transform.position, Vector3.Distance(transform.position, player.transform.position), wallMask); }
    }

    public State GuardState
    {
        get { return state; }
    }
}
