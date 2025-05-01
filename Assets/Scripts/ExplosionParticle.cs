using UnityEngine;

public class ExplosionParticle : MonoBehaviour
{
    private ParticleSystem explosion;

    void Start()
    {
        explosion = GetComponent<ParticleSystem>();
        if (explosion != null)
        {
            explosion.Play();
        }
        if(gameObject != null)
        {
            Destroy(gameObject, 3f);
        }
        
    }
}
