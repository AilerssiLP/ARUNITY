using UnityEngine;

public class StartupUIManager : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject mainMenuPanel;

    public void StartExperience()
    {
        startPanel.SetActive(false);    
        mainMenuPanel.SetActive(true);    
    }
}
