using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float speed = 10;
    public LayerMask wallMask;
    public LayerMask guardMask;
    public LayerMask playerMask;

    private Vector3 dir;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(5);
        GetComponent<TrailRenderer>().Clear();
        gameObject.SetActive(false);
    }

    public void Setup(Vector3 dir)
	{
        this.dir = dir.normalized;
	}

	void Update()
	{
        Vector3 velocity = dir * speed * Time.deltaTime;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity, out hit, velocity.magnitude, wallMask))
            if (hit.collider != null)
            {
                StopAllCoroutines();
                GetComponent<TrailRenderer>().Clear();
                gameObject.SetActive(false);
            }

        if (Physics.Raycast(transform.position, velocity, out hit, velocity.magnitude, guardMask))
            if (hit.collider != null)
            {
                hit.collider.GetComponent<Guard>().Hit();
                StopAllCoroutines();
                GetComponent<TrailRenderer>().Clear();
                gameObject.SetActive(false);
            }

        if (Physics.Raycast(transform.position, velocity, out hit, velocity.magnitude, playerMask))
            if (hit.collider != null)
            {
                hit.collider.GetComponent<Player>().Hit();
                StopAllCoroutines();
                GetComponent<TrailRenderer>().Clear();
                gameObject.SetActive(false);
            }

        transform.Translate(velocity, Space.World);
	}
}
