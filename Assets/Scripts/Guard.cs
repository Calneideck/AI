using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Guard : MonoBehaviour, IPather
{
    public enum State { PATROLLING, SEEKING, ENGAGING };
    private State state = State.PATROLLING;

    private Vector3[] path;
    private int pathIndex;
    private Player player;
    private BehaviourTree bTree;
    private float detection = 0;
    private float startTaskTime;
    private float startRotation;
    private Vector3 lastPlayerSightPos;
    private Transform alertedGuard = null;
    private Vector3 investigateLocation = Vector3.zero;
    private int health = 100;

    public float speed = 4;
    public float minSightRange = 2.5f;
    public float maxSightRange = 8;
    public GameObject bulletPrefab;
    public Transform canvas;
    public Image detectionImage;
    public RawImage healthImage;
    public Text stateText;
    public LayerMask wallMask;

    public static List<Guard> allGuards = new List<Guard>();

    void Start()
    {
        bTree = GetComponent<BehaviourTree>();
        ObjectPooler.instance.Setup(bulletPrefab, 2);
        player = GameObject.Find("Player").GetComponent<Player>();
        minSightRange *= minSightRange;
        maxSightRange *= maxSightRange;
        allGuards.Add(this);
    }

    void Update()
    {
        switch (state)
        {
            case State.PATROLLING:
                bTree.Patrol.Update();
                Detect();
                break;
            case State.SEEKING:
                bTree.Seek.Update();
                Detect();
                break;
            case State.ENGAGING:
                bTree.Engage.Update();
                break;
        }
        
        canvas.rotation = Quaternion.Euler(Vector3.right * 90);
    }

    void Detect()
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
            ChangeState(State.ENGAGING);
        }
        else if (state == State.PATROLLING)
        {
            foreach (Guard guard in allGuards)
                if (guard != this)
                    if (Vector3.Angle(guard.transform.position - transform.position, transform.forward) < 60)
                        if ((guard.transform.position - transform.position).sqrMagnitude < maxSightRange)
                        {
                            if (!Physics.Raycast(transform.position, guard.transform.position - transform.position, Vector3.Distance(transform.position, guard.transform.position), wallMask))
                                if (guard.GuardState != State.PATROLLING)
                                {
                                    ChangeState(State.SEEKING);
                                    alertedGuard = guard.transform;
                                    break;
                                }
                        }
        }

        detectionImage.fillAmount = detection;
    }

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

    public void HeardGunshot(Vector3 position)
    {
        if (state == State.PATROLLING)
        {
            investigateLocation = position;
            ChangeState(State.SEEKING);
        }
        else if (state == State.SEEKING)
        {
            investigateLocation = position;
            Pathfinding.instance.RequestPath(gameObject, position);
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

    public void ChangeState(State state)
    {
        this.state = state;
        switch (state)
        {
            case State.PATROLLING:
                bTree.Patrol.Reset();
                alertedGuard = null;
                investigateLocation = Vector3.zero;
                break;
            case State.SEEKING:
                bTree.Seek.Reset();
                break;
            case State.ENGAGING:
                bTree.Engage.Reset();
                alertedGuard = null;
                investigateLocation = Vector3.zero;
                break;
        }
        stateText.text = state.ToString();
    }

    public void Hit()
    {
        if (health <= 0)
            return;

        health -= 15;
        if (state == State.PATROLLING)
            health = 0;

        if (health <= 0)
        {
            allGuards.Remove(this);
            GameObject.Destroy(gameObject);
            health = 0;
        }
        else
            healthImage.rectTransform.sizeDelta = new Vector2(3 * health / 100f, 0.4f);
    }

    public void OnSearch()
    {
        startRotation = transform.rotation.eulerAngles.y;
        startTaskTime = Time.time;
    }

    public bool CanSeePlayer
    {
        get { return !Physics.Raycast(transform.position, player.transform.position - transform.position, Vector3.Distance(transform.position, player.transform.position), wallMask); }
    }

    public State GuardState
    {
        get { return state; }
    }

    public float StartTaskTime
    {
        get { return startTaskTime; }
    }

    public Vector3[] Path
    {
        get { return path; }
    }

    public int PathIndex
    {
        get { return pathIndex; }
        set { pathIndex = value; }
    }

    public Vector3 InvestigateLocation
    {
        get { return investigateLocation; }
    }

    public Transform AlertedGuard
    {
        get { return alertedGuard; }
    }

    public float StartRotation
    {
        get { return startRotation; }
    }
}
