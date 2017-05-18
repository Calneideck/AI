using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    public float speed = 2;
    public LayerMask wallMask;
    public GameObject bulletPrefab;
    public float shootCooldown = 0.1f;

    private float lastShootTime;

    void Start()
    {
        ObjectPooler.instance.Setup(bulletPrefab, 20);
	}

	void Update()
	{
        Move();
        Shoot();
	}

    void Shoot()
    {
        if (Input.GetButton("Fire1") && Time.time - lastShootTime >= shootCooldown)
        {
            GameObject bullet = ObjectPooler.instance.GetObject(bulletPrefab);
            bullet.transform.position = transform.position + transform.forward * 0.4f;
            bullet.GetComponent<Bullet>().Setup(transform.forward);
            lastShootTime = Time.time;
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
}
