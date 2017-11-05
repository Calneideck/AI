using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    public float speed = 2;
    public LayerMask wallMask;
    public Text healthText;
    public Text gunText;

    [Header("Pistol")]
    public GameObject pistolBulletPrefab;
    public float pistolCooldown = 1.2f;
    public AudioSource pistolSound;
    public GameObject pistolObj;

    [Header("Laser")]
    public GameObject laserBulletPrefab;
    public float laserCooldown = 0.1f;
    public AudioSource laserSound;
    public GameObject laserObj;
    public float gunShotSoundRange = 10;

    private bool alive = true;
    private float lastShootTime;
    private bool usingLaser;
    private int health = 100;

    void Start()
    {
        ObjectPooler.instance.Setup(laserBulletPrefab, 20);
        ObjectPooler.instance.Setup(pistolBulletPrefab, 2);
        gunShotSoundRange *= gunShotSoundRange;
	}

	void Update()
	{
        if (Input.GetKeyDown(KeyCode.P))
            Time.timeScale = 1 - Time.timeScale;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Boid.Boids.Clear();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        Move();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            usingLaser = !usingLaser;
            laserObj.SetActive(usingLaser);
            pistolObj.SetActive(!usingLaser);
            gunText.text = usingLaser ? "Laser" : "Pistol";
        }

        Shoot();
	}

    void Shoot()
    {
        if (Input.GetButton("Fire1") && Time.time - lastShootTime >= (usingLaser ? laserCooldown : pistolCooldown))
        {
            GameObject bullet = ObjectPooler.instance.GetObject(usingLaser ? laserBulletPrefab : pistolBulletPrefab);
            bullet.transform.position = transform.position + transform.forward * 0.4f;
            bullet.GetComponent<Bullet>().Setup(transform.forward);
            lastShootTime = Time.time;
            (usingLaser ? laserSound : pistolSound).Play();

            if (usingLaser)
                foreach (Guard guard in Guard.allGuards)
                    if ((guard.transform.position - transform.position).sqrMagnitude < gunShotSoundRange)
                        guard.HeardGunshot(transform.position);
        }
    }

    void Move()
    {
        Vector3 velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * speed * Time.deltaTime;

        Vector3 heading = GetMousePositionOnXZPlane() - transform.position;
        transform.rotation = Quaternion.Euler(0, Mathf.Atan2(heading.x, heading.z) * Mathf.Rad2Deg, 0);

        RaycastHit wallHit;
        if (Physics.Raycast(transform.position, velocity, out wallHit, velocity.magnitude + 0.2f, wallMask))
            if (wallHit.collider != null)
                velocity = Vector3.zero;

        transform.Translate(velocity, Space.World);
    }

    Vector3 GetMousePositionOnXZPlane()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane XZPlane = new Plane(Vector3.up, Vector3.up * 0.4f);
        if (XZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            hitPoint.y = 0;
            return hitPoint;
        }
        return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (DebugHelper.gunSoundRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? Mathf.Sqrt(gunShotSoundRange) : gunShotSoundRange);
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Boid")
        {
            coll.GetComponent<Boid>().Dead();
            if (health <= 0)
                return;

            health -= 2;
            if (health <= 0)
                GameObject.Destroy(gameObject);

            healthText.text = "Health: " + health.ToString();
        }
    }

    public void Hit()
    {
        if (health <= 0)
            return;

        health -= 20;
        if (health <= 0)
            GameObject.Destroy(gameObject);

        healthText.text = "Health: " + health.ToString();
    }

    public bool Alive
    {
        get { return alive; }
    }
}
