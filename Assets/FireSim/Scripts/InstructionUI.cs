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
                @"?? In�trukcie pre in�truktora:

1. Vyber objekt na pravej strane.
2. Klepnut�m na rovn� povrch ho umiestni.
3. Pou�i tla�idlo �Zobrazi� menu� pre �al�ie akcie.
 4. M��e� objekt zdvihn��, presun��, ot��a� alebo zmaza�.
5. Spusti simul�ciu po�iaru pomocou tla�idla �Sprej�.";
        }
        else
        {
            instructionsText.text =
                @"?? In�trukcie pre tr�ning:

1. Presu� sa okolo objektov a vn�maj ich umiestnenie.
2. M��e� zdvihn�� a ot��a� len objekty ozna�en� ako �Pickup�.
3. Simul�cia po�iaru je dostupn� pre �peci�lne objekty.
4. V�etky edita�n� mo�nosti s� zablokovan�.";
        }
    }
    public void RefreshInstructions()
    {
        if (!isPanelVisible) return;

        if (ARTemplateMenuManager.CurrentMode == ARTemplateMenuManager.AppMode.Instructor)
        {
            instructionsText.text =
                @"?? In�trukcie pre in�truktora:

1. Vyber objekt na pravej strane.
2. Klepnut�m na rovn� povrch ho umiestni.
3. Pou�i tla�idlo �Zobrazi� menu� pre �al�ie akcie.
4. M��e� objekt zdvihn��, presun��, ot��a� alebo zmaza�.
5. Spusti simul�ciu po�iaru pomocou tla�idla �Sprej�.";
        }
        else
        {
            instructionsText.text =
                @"?? In�trukcie pre tr�ning:

1. Presu� sa okolo objektov a vn�maj ich umiestnenie.
2. M��e� zdvihn�� a ot��a� len objekty ozna�en� ako �Pickup�.
3. Simul�cia po�iaru je dostupn� pre �peci�lne objekty.
4. V�etky edita�n� mo�nosti s� zablokovan�.";
        }
    }


    public void HideInstructions()
    {
        isPanelVisible = false;
        instructionsPanel.SetActive(false);
        Debug.Log("[Instructions] Panel hidden");
    }
}
