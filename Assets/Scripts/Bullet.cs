using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float customGravity = 2f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
    }
}
