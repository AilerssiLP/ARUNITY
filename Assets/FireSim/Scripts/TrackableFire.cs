using UnityEngine;

public class TrackableFire : MonoBehaviour
{
    public enum FireType { ClassA, ClassB, ClassC }
    public FireType fireType;
    public bool mistakeAlreadyLogged = false;
    public bool HasMistakeLogged { get; set; } = false;

    public bool IsExtinguished { get; private set; } = false;
    void Start()
    {
        if (ARTemplateMenuManager.CurrentMode == ARTemplateMenuManager.AppMode.Training && TrainingStatsManager.Instance != null)
        {
            TrainingStatsManager.Instance.RegisterFire(this);
        }
    }

    public void Extinguish(bool correctExt, FireType usedExtinguisherType)
    {
        IsExtinguished = true;
        TrainingStatsManager.ReportExtinguished(this, correctExt, usedExtinguisherType);
        FindAnyObjectByType<ARTemplateMenuManager>()?.UpdateStatsText();
    }
    public void ResetFire()
    {
        IsExtinguished = false;
        HasMistakeLogged = false;

        if (TryGetComponent<Fire>(out var fire))
        {
            fire.ResetIntensity();
        }
    }
}
