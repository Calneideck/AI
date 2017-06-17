using UnityEngine;
using System.Collections;

public class BoidController : MonoBehaviour
{
    [System.Serializable()]
    public struct Spawn
    {
        public Transform spawnPos;
        public int amount;

        public Spawn(Transform spawnPos, int amount)
        {
            this.spawnPos = spawnPos;
            this.amount = amount;
        }
    }

    public Player player;
    [SerializeField]
    private float _neighbourRange = 4;
    [SerializeField]
    private float _maxSpeed = 10;
    [SerializeField]
    private float _wander = 1;
    [SerializeField]
    private float _cohesion = 1;
    [SerializeField]
    private float _alignment = 1;
    [SerializeField]
    private float _separation = 1;
    [SerializeField]
    private float _wallAvoidance = 1;

    [Header("Wander")]
    [SerializeField]
    private float _wanderDist = 2;
    [SerializeField]
    private float _wanderRadius = 1;
    [SerializeField]
    private float _wanderJitter = 5;

    public static float neighbourRange;
    public static float maxSpeed;
    public static float wander;
    public static float cohesion;
    public static float alignment;
    public static float separation;
    public static float wallAvoidance;

    public static float wanderDist;
    public static float wanderRadius;
    public static float wanderJitter;

    [Header("Spawning")]
    public Spawn[] spawns;
    public float spawnRadius = 1.5f;
    public GameObject boidPrefab;
    public Color[] colours;

    void Start()
    {
        foreach (Spawn spawn in spawns)
            for (int i = 0; i < spawn.amount; i++)
            {
                Vector3 pos = spawn.spawnPos.position + Random.insideUnitSphere * spawnRadius;
                pos.y = Random.Range(0.1f, 0.7f);
                GameObject boid = GameObject.Instantiate(boidPrefab, pos, Random.rotationUniform, transform);
                boid.GetComponentInChildren<TrailRenderer>().startColor = colours[Random.Range(0, colours.Length)];
                boid.GetComponentInChildren<TrailRenderer>().endColor = colours[Random.Range(0, colours.Length)];
                boid.GetComponent<Boid>().Setup(player);
            }
    }

    void OnValidate()
    {
        neighbourRange = _neighbourRange;
        maxSpeed = _maxSpeed;
        wander = _wander;
        cohesion = _cohesion;
        alignment = _alignment;
        separation = _separation;
        wallAvoidance = _wallAvoidance;

        wanderDist = _wanderDist;
        wanderRadius = _wanderRadius;
        wanderJitter = _wanderJitter;
    }

    void Awake()
    {
        neighbourRange = _neighbourRange;
        maxSpeed = _maxSpeed;
        wander = _wander;
        cohesion = _cohesion;
        alignment = _alignment;
        separation = _separation;
        wallAvoidance = _wallAvoidance;

        wanderDist = _wanderDist;
        wanderRadius = _wanderRadius;
        wanderJitter = _wanderJitter;
    }
}
