using System.Collections;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    private Transform target;
    public float speed = 50f;
    public float rotateSpeed = 5f;
    private ParticleSystem fuseParticle;
    private bool isFuseActive = false;

    private float hitDistance = 1f;
    public GameObject hitParticle;

    private void Start()
    {
        fuseParticle = GetComponentInChildren<ParticleSystem>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        TrackDistance();    
    }

    void FixedUpdate()
    {
        if (target == null) return;
        if(isFuseActive == false)
        {
            isFuseActive = true;
            StartCoroutine(DestroyMissile());
            fuseParticle.Play();
        }
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private IEnumerator DestroyMissile()
    {
        yield return new WaitForSeconds(30f);
        if (fuseParticle != null)
        {
            fuseParticle.Stop();
        }
        Destroy(gameObject, 1f);
    }

    private void TrackDistance()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= hitDistance)
        {
            EnemyAirCraft enemy = target.GetComponent<EnemyAirCraft>();
            if (enemy != null)
            {
                enemy.ShootDown();
            }

            Instantiate(hitParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
