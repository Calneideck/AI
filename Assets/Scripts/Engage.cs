using UnityEngine;
using System.Collections;

public class Engage : MonoBehaviour
{
    private float lastShootTime;
    private int ammo = 5;
    private Vector3 lastPlayerSightPos;
    private Player player;

    public GameObject bulletPrefab;
    public Patrol patrol;
    public Guard guard;
    public float reloadTime = 1.8f;
    public float shootCooldown = 0.9f;
    public LayerMask wallMask;
    public AudioSource audioSource;
    public float gunShotSoundRange;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    public BNode.ResultState Pursue()
    {
        // fire at player
        if (Time.time - lastShootTime >= shootCooldown && guard.CanSeePlayer)
        {
            GameObject bullet = ObjectPooler.instance.GetObject(bulletPrefab);
            bullet.transform.position = transform.position + transform.forward * 0.4f;
            Vector3 shootTarget = player.transform.position - transform.position;
            shootTarget.y = 0;
            bullet.GetComponent<Bullet>().Setup(shootTarget);
            lastShootTime = Time.time;
            audioSource.Play();
            foreach (Guard guard in Guard.allGuards)
                if ((guard.transform.position - transform.position).sqrMagnitude < gunShotSoundRange)
                    guard.HeardGunshot(transform.position);
            if (--ammo == 0)
                return BNode.ResultState.FAILURE;
        }

        if (guard.PathIndex < guard.Path.Length)
        {
            Vector3 moveTarget = guard.Path[guard.PathIndex];
            // Move at 60% speed while pursuing
            transform.Translate((moveTarget - transform.position).normalized * player.speed * 0.6f * Time.deltaTime, Space.World);
            if (guard.CanSeePlayer)
            {
                lastPlayerSightPos = player.transform.position;
                transform.LookAt(player.transform);
            }
            else
                transform.LookAt(moveTarget);

            if ((moveTarget - transform.position).sqrMagnitude < 0.04f)
            {
                guard.PathIndex++;
                if (guard.PathIndex == guard.Path.Length)
                    Pathfinding.instance.RequestPath(gameObject, player.transform.position);
            }
        }

        return BNode.ResultState.RUNNING;
    }

    public void OnPursue()
    {
        if (guard.CanSeePlayer)
            Pathfinding.instance.RequestPath(gameObject, player.transform.position);
        else
            Pathfinding.instance.RequestPath(gameObject, lastPlayerSightPos);
    }

    public BNode.ResultState SeekCover()
    {
        BNode.ResultState result = patrol.FollowPath();
        if (result == BNode.ResultState.RUNNING)
            if (Physics.Raycast(transform.position, player.transform.position - transform.position, Vector3.Distance(transform.position, player.transform.position), wallMask))
                result = BNode.ResultState.SUCCESS;

        return result;
    }

    public void OnSeekCover()
    {
        Vector3 coverPos = Pathfinding.instance.GetHidingPos(gameObject, player.gameObject, wallMask);
        Pathfinding.instance.RequestPath(gameObject, coverPos);
    }

    public BNode.ResultState Reload()
    {
        if (Time.time - guard.StartTaskTime >= reloadTime)
        {
            ammo = 5;
            return BNode.ResultState.SUCCESS;
        }

        return BNode.ResultState.RUNNING;
    }
}
