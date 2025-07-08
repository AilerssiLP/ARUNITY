using UnityEngine;

public class Fire : MonoBehaviour
{
    public float currentIntensity { get; private set; } = 1f;

    private float[] startIntensities = new float[0];

    float timeLastWatered = 0f;

    [SerializeField] private float regenDelay = 2.5f;
    [SerializeField] private float regenRate = 0.1f;

    [SerializeField] private ParticleSystem[] fireParticleSystems = new ParticleSystem[0];

    [SerializeField] private float maxIntensity = 1f;

    private bool isLit = true;

    private void Start()
    {
        startIntensities = new float[fireParticleSystems.Length];

        for (int i = 0; i < fireParticleSystems.Length; i++)
        {
            if (fireParticleSystems[i] != null)
            {
                startIntensities[i] = fireParticleSystems[i].emission.rateOverTime.constant;
            }
        }
    }

    public void AddIntensity(float amount)
    {
        currentIntensity += amount;
        currentIntensity = Mathf.Clamp01(currentIntensity);
    }

    private void Update()
    {
        if (isLit && currentIntensity < 1.0f && Time.time - timeLastWatered >= regenDelay)
        {
            currentIntensity += regenRate * Time.deltaTime;
            currentIntensity = Mathf.Clamp01(currentIntensity);
            ChangeIntensity();
        }
    }

    public bool TryExtinguish(float amount)
    {
        timeLastWatered = Time.time;

        currentIntensity -= amount;
        currentIntensity = Mathf.Clamp01(currentIntensity);
        ChangeIntensity();

        if (currentIntensity <= 0)
        {
            isLit = false;
            return true;
        }

        return false;
    }

    private void ChangeIntensity()
    {
        for (int i = 0; i < fireParticleSystems.Length; i++)
        {
            if (fireParticleSystems[i] == null) continue;

            var emission = fireParticleSystems[i].emission;
            emission.rateOverTime = currentIntensity * startIntensities[i];
        }
    }

    public bool IsFullyExtinguished()
    {
        return currentIntensity <= 0f;
    }
    public void ResetIntensity()
    {
        currentIntensity = maxIntensity;
        isLit = true;
        timeLastWatered = Time.time;

        for (int i = 0; i < fireParticleSystems.Length; i++)
        {
            if (fireParticleSystems[i] == null) continue;

            fireParticleSystems[i].Clear();
            fireParticleSystems[i].Play();

            var emission = fireParticleSystems[i].emission;
            emission.rateOverTime = currentIntensity * maxIntensity;
        }
    }

}
