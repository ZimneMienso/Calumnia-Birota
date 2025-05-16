using UnityEngine;

public class BloodExplosion : MonoBehaviour
{
    [SerializeField] ParticleSystem explosion;

    private void Awake()
    {
        explosion.Play();
    }

    void Update()
    {
        if (!explosion.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
