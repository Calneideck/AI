using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour, IPather
{
    public struct Group
    {
        public int count;
        public Boid boid;

        public Group(int count, Boid boid)
        {
            this.count = count;
            this.boid = boid;
        }
    }

    private enum BoidState { FLOCKING, SEARCHING, ENGAGING };
    private BoidState state = BoidState.FLOCKING;

    private Quaternion[] rotations = new Quaternion[] { Quaternion.identity, Quaternion.Euler(0, 30, 0), Quaternion.Euler(0, -30, 0), Quaternion.Euler(30, 0, 0), Quaternion.Euler(-30, 0, 0) };

    private Vector3 vel;
    private List<Boid> neighbours = new List<Boid>();
    private Vector3 wanderTarget = Vector3.forward;
    private float awayTime;
    private Vector3[] path;
    private int pathIndex;
    private Player player;

    public LayerMask wallMask;
    public float searchSpeed;
    public float engageSpeed = 3;

    public static List<Boid> Boids = new List<Boid>();
    public static Group LargestGroup = new Group(0, null);

    void Awake()
    {
        Boids.Add(this);
    }

    public void Setup(Player player)
    {
        this.player = player;
    }

    void Update()
    {
        switch (state)
        {
            case BoidState.FLOCKING:
                Flocking();
                if (CanSeePlayer)
                    state = BoidState.ENGAGING;
                break;

            case BoidState.SEARCHING:
                GetNeighbours();
                if (neighbours.Count > 0)
                    state = BoidState.FLOCKING;
                else
                    FollowPath();

                if (CanSeePlayer)
                    state = BoidState.ENGAGING;
                break;

            case BoidState.ENGAGING:
                if (CanSeePlayer)
                    transform.Translate((player.transform.position - transform.position).normalized * Time.deltaTime * engageSpeed, Space.World);
                else
                    state = BoidState.FLOCKING;
                break;
        }


        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, 0.08f, 0.72f);
        transform.position = pos;
    }

    void Flocking()
    {
        GetNeighbours();
        if (neighbours.Count == 0)
        {
            awayTime += Time.deltaTime;
            if (awayTime >= 2)
            {
                if (Pathfinding.instance && LargestGroup.boid)
                    Pathfinding.instance.RequestPath(gameObject, LargestGroup.boid.transform.position);
                state = BoidState.SEARCHING;
            }
        }
        else
            awayTime = 0;


        Vector3 accel = Force(); // Mass is irrelvant here
        vel += accel;
        vel = Vector3.ClampMagnitude(vel, BoidController.maxSpeed);
        transform.LookAt(transform.position + vel);
        transform.position += vel * Time.deltaTime;
    }

    Vector3 Force()
    {
        Vector3 force = Wander() * BoidController.wander;
        force += Cohesion() * BoidController.cohesion;
        force += Alignment() * BoidController.alignment;
        force += Separation() * BoidController.separation;
        force += WallAvoidance() * BoidController.wallAvoidance;
        return force;
    }

    void GetNeighbours()
    {
        neighbours.Clear();
        foreach (Boid boid in Boids)
            if (boid != this)
            {
                if ((boid.transform.position - transform.position).sqrMagnitude < Mathf.Pow(BoidController.neighbourRange, 2))
                    neighbours.Add(boid);
            }

        if (LargestGroup.boid == this || neighbours.Count > LargestGroup.count)
        {
            LargestGroup.count = neighbours.Count;
            LargestGroup.boid = this;
        }
    }

    Vector3 Wander()
    {
        wanderTarget += Random.insideUnitSphere * BoidController.wanderJitter * Time.deltaTime;
        wanderTarget.Normalize();
        Vector3 worldPoint = transform.position + transform.forward * BoidController.wanderDist + wanderTarget * BoidController.wanderRadius;
        return (worldPoint - transform.position).normalized;
    }

    void OnDrawGizmos()
    {
        if (DebugHelper.wander)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position + transform.forward * BoidController.wanderDist, BoidController.wanderRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.forward * BoidController.wanderDist + wanderTarget * BoidController.wanderRadius, 0.2f);
        }
    }

    Vector3 Cohesion()
    {
        Vector3 centreOfMass = Vector3.zero;
        if (neighbours.Count > 0)
        {
            foreach (Boid boid in neighbours)
                centreOfMass += boid.transform.position;

            centreOfMass /= neighbours.Count;
        }

        return (centreOfMass - transform.position).normalized;
    }

    Vector3 Alignment()
    {
        Vector3 avgHeading = Vector3.zero;
        if (neighbours.Count > 0)
        {
            foreach (Boid boid in neighbours)
                avgHeading += boid.Heading;

            avgHeading /= neighbours.Count;
            avgHeading -= Heading;
        }

        return avgHeading;
    }

    Vector3 Separation()
    {
        Vector3 force = Vector3.zero;
        if (neighbours.Count > 0)
            foreach (Boid boid in neighbours)
            {
                Vector3 awayForce = transform.position - boid.transform.position;
                force += awayForce.normalized / awayForce.magnitude;
            }

        return force.normalized;
    }

    Vector3 WallAvoidance()
    {
        Vector3 force = Vector3.zero;

        foreach (Quaternion rotation in rotations)
        {
            RaycastHit wallHit;
            if (Physics.Raycast(transform.position, rotation * transform.forward, out wallHit, 0.5f, wallMask))
                force += wallHit.normal * (0.5f - wallHit.distance);
        }

        return force;
    }

    void FollowPath()
    {
        if (path == null)
            return;

        if (pathIndex < path.Length)
        {
            Vector3 target = path[pathIndex];
            target.y = 0.3f;
            transform.Translate((target - transform.position).normalized * searchSpeed * Time.deltaTime, Space.World);
            transform.LookAt(target);

            if (Vector3.Distance(transform.position, target) < 0.2f)
            {
                pathIndex++;
                if (pathIndex == path.Length)
                    Pathfinding.instance.RequestPath(gameObject, LargestGroup.boid.transform.position);
            }
        }
    }

    public void Dead()
    {
        Boids.Remove(this);
        if (LargestGroup.boid == this)
        {
            LargestGroup.count = 0;
            if (Boids.Count > 0)
                LargestGroup.boid = Boids[0];
        }
        GameObject.Destroy(gameObject);
    }

    public void PathReceived(Vector3[] path)
    {
        if (path.Length > 0)
        {
            this.path = path;
            pathIndex = 0;
        }
    }

    public Vector3 Heading
    {
        get { return vel.normalized; }
    }

    bool CanSeePlayer
    {
        get { return !Physics.Raycast(transform.position, player.transform.position - transform.position, Vector3.Distance(transform.position, player.transform.position), wallMask); }
    }
}
