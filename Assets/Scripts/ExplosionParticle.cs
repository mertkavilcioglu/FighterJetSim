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

        Destroy(gameObject, 3f); 
    }
}
