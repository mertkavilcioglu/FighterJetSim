using UnityEngine;

public class MissileController : MonoBehaviour
{
    private Transform target;
    public float speed = 50f;
    public float rotateSpeed = 5f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
