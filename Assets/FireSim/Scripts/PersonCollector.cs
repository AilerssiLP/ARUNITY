using UnityEngine;
using static ARTemplateMenuManager;

public class PersonCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Person"))
            return;

        if (ARTemplateMenuManager.CurrentMode == AppMode.Training)
        {
            other.gameObject.SetActive(false);
            FindFirstObjectByType<ARTemplateMenuManager>().SavePerson();
        }
    }
}
