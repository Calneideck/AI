using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float speed = 10;
    public LayerMask wallMask;

    private Vector3 dir;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(5);
        GameObject.Destroy(gameObject);
    }

    public void Setup(Vector3 dir)
	{
        this.dir = dir.normalized;
	}

	void Update()
	{
        Vector3 velocity = dir * speed * Time.deltaTime;

        RaycastHit wallHit;
        if (Physics.Raycast(transform.position, velocity, out wallHit, velocity.magnitude + 0.05f, wallMask))
            if (wallHit.collider != null)
            {
                StopAllCoroutines();
                GetComponent<TrailRenderer>().Clear();
                gameObject.SetActive(false);
            }

        transform.Translate(velocity);
	}
}
