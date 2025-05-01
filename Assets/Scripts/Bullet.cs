using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float customGravity = 1f;
    private Rigidbody rb;
    public GameObject mainObj;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        StartCoroutine(DestroyBullet());
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyAirCraft>().ShootDown();
        }
    }

    private IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(3f);
        Destroy(mainObj);
    }
}
