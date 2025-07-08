using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EscapeDoor : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (ARTemplateMenuManager.CurrentMode != ARTemplateMenuManager.AppMode.Training) return;

        if (other.transform.GetComponentInParent<ARCameraManager>() != null)
        {
            Debug.Log("?? Exit triggered by AR camera or hand anchor");
            FindAnyObjectByType<ARTemplateMenuManager>()?.StopTraining();
        }
    }

}
