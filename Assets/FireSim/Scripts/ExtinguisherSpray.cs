using UnityEngine;

public class ExtinguisherSpray : MonoBehaviour
{
    [SerializeField] private ParticleSystem sprayParticles;
    [SerializeField] private AudioSource spraySound;

    public void Spray()
    {
        if (sprayParticles != null && !sprayParticles.isPlaying)
        {
            sprayParticles.Play();
        }

        if (spraySound != null && !spraySound.isPlaying)
        {
            spraySound.Play();
        }
    }

    public void StopSpray()
    {
        if (sprayParticles != null && sprayParticles.isPlaying)
        {
            sprayParticles.Stop();
        }

        if (spraySound != null && spraySound.isPlaying)
        {
            spraySound.Stop();
        }
    }
}
