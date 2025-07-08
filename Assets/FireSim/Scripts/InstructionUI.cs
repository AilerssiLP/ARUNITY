using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstructionUI : MonoBehaviour
{
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private TMP_Text instructionsText;
    [SerializeField] private ARTemplateMenuManager modeManager;

    private bool isPanelVisible = false;

    void Start()
    {
        instructionsPanel.SetActive(false); 
    }

    void Update()
    {
        if (isPanelVisible && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            ToggleInstructions();
        }
    }

    public void ToggleInstructions()
    {
        if (isPanelVisible)
        {
            HideInstructions();
        }
        else
        {
            ShowInstructions();
        }
    }

    void ShowInstructions()
    {
        isPanelVisible = true;
        instructionsPanel.SetActive(true);

        Debug.Log("[Instructions] Panel shown");

        if (ARTemplateMenuManager.CurrentMode == ARTemplateMenuManager.AppMode.Instructor)
        {
            instructionsText.text =
                @"?? Inštrukcie pre inštruktora:

1. Vyber objekt na pravej strane.
2. Klepnutím na rovnı povrch ho umiestni.
3. Poui tlaèidlo „Zobrazi menu“ pre ïalšie akcie.
 4. Môeš objekt zdvihnú, presunú, otáèa alebo zmaza.
5. Spusti simuláciu poiaru pomocou tlaèidla „Sprej“.";
        }
        else
        {
            instructionsText.text =
                @"?? Inštrukcie pre tréning:

1. Presuò sa okolo objektov a vnímaj ich umiestnenie.
2. Môeš zdvihnú a otáèa len objekty oznaèené ako „Pickup“.
3. Simulácia poiaru je dostupná pre špeciálne objekty.
4. Všetky editaèné monosti sú zablokované.";
        }
    }
    public void RefreshInstructions()
    {
        if (!isPanelVisible) return;

        if (ARTemplateMenuManager.CurrentMode == ARTemplateMenuManager.AppMode.Instructor)
        {
            instructionsText.text =
                @"?? Inštrukcie pre inštruktora:

1. Vyber objekt na pravej strane.
2. Klepnutím na rovnı povrch ho umiestni.
3. Poui tlaèidlo „Zobrazi menu“ pre ïalšie akcie.
4. Môeš objekt zdvihnú, presunú, otáèa alebo zmaza.
5. Spusti simuláciu poiaru pomocou tlaèidla „Sprej“.";
        }
        else
        {
            instructionsText.text =
                @"?? Inštrukcie pre tréning:

1. Presuò sa okolo objektov a vnímaj ich umiestnenie.
2. Môeš zdvihnú a otáèa len objekty oznaèené ako „Pickup“.
3. Simulácia poiaru je dostupná pre špeciálne objekty.
4. Všetky editaèné monosti sú zablokované.";
        }
    }


    public void HideInstructions()
    {
        isPanelVisible = false;
        instructionsPanel.SetActive(false);
        Debug.Log("[Instructions] Panel hidden");
    }
}
