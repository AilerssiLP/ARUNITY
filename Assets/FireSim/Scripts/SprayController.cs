using UnityEngine;

public class SprayController : MonoBehaviour
{
    [SerializeField] ParticleSystem sprayEffect;
    [SerializeField] private Transform sprayOrigin;
    public enum SprayType { CO2, Water, Powder }
    public SprayType type = SprayType.CO2;

    public bool isSpraying { get; private set; } = false;

    [SerializeField] private float extinguishPower = 1f;

    public void StartSpray()
    {
        if (sprayEffect == null) return;

        sprayEffect.Play();
        isSpraying = true;
        Debug.Log($"[SprayController] Started spraying ({type})");
    }

    public void StopSpray()
    {
        if (sprayEffect == null) return;

        sprayEffect.Stop();
        isSpraying = false;
        Debug.Log("[SprayController] Stopped spraying");
    }

    void Update()
    {
        if (isSpraying)
        {
            UpdateSprayExtinguishing();
        }
    }

    public void UpdateSprayExtinguishing()
    {
        if (Physics.Raycast(sprayOrigin.position, sprayOrigin.forward, out RaycastHit hit, 10f))
        {
            Debug.Log($"[Raycast] Hit: {hit.collider.name}");

            if (hit.collider.TryGetComponent<Fire>(out Fire fire))
            {
                if (hit.collider.TryGetComponent<TrackableFire>(out TrackableFire trackable))
                {
                    Debug.Log($"[Trackable] TrackableFire found. IsExtinguished: {trackable.IsExtinguished}");

                    bool correct = IsCorrectExtinguisher(trackable.fireType, type);
                    Debug.Log($"[Trackable] Is correct extinguisher? {correct}");

                    if (correct)
                    {
                        fire.TryExtinguish(extinguishPower * Time.deltaTime);
                        Debug.Log($"[Fire] Extinguishing with correct extinguisher. Intensity: {fire.currentIntensity}");

                        if (fire.IsFullyExtinguished() && !trackable.IsExtinguished)
                        {
                            trackable.Extinguish(true, GetExtinguisherFireType(type));

                            Debug.Log("[Trackable] Fully extinguished correctly.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Trackable] Incorrect extinguisher. No effect.");

                        if (!trackable.IsExtinguished)
                        {
                            trackable.Extinguish(false, GetExtinguisherFireType(type));

                        }
                    }
                }
                else
                {
                    fire.TryExtinguish(extinguishPower * Time.deltaTime);
                }
            }
            else
            {
                Debug.LogWarning("[Fire] No Fire script found on hit.");
            }
        }
        else
        {
            Debug.LogWarning("[Raycast] No hit detected.");
        }
    }

    bool IsCorrectExtinguisher(TrackableFire.FireType fireType, SprayType sprayType)
    {
        return (fireType == TrackableFire.FireType.ClassA && sprayType == SprayType.Water)
            || (fireType == TrackableFire.FireType.ClassB && sprayType == SprayType.Powder)
            || (fireType == TrackableFire.FireType.ClassC && sprayType == SprayType.CO2);
    }

    TrackableFire.FireType GetExtinguisherFireType(SprayType spray)
    {
        return spray switch
        {
            SprayType.CO2 => TrackableFire.FireType.ClassC,
            SprayType.Water => TrackableFire.FireType.ClassA,
            SprayType.Powder => TrackableFire.FireType.ClassB,
            _ => TrackableFire.FireType.ClassA
        };
    }

    void OnDrawGizmos()
    {
        if (sprayOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(sprayOrigin.position, sprayOrigin.forward * 10f);
        }
    }
}
