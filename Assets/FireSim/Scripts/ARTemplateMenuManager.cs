using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;


public class ARTemplateMenuManager : MonoBehaviour
{
    public enum AppMode
    {
        Instructor,
        Training
    }

    void UpdateModeText()
    {
        if (modeStatusText != null)
        {
            string localized = CurrentMode == AppMode.Instructor ? "Režim Inštruktora" : "Režim Trénovania";
            modeStatusText.text = localized;
        }
    }
    private int savedPeopleCount = 0;
    public ScreenSpaceSelectInput selectInput;
    public ScreenSpaceRotateInput rotateInput;
    public ScreenSpacePinchScaleInput scaleInput;

    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text trainingTimerText;


    private float trainingTimer = 0f;
    private bool isTrainingModeActive = false;

    [SerializeField] XRRayInteractor m_RayInteractor;

    [SerializeField] private TMP_Text modeStatusText;

    [SerializeField] MonoBehaviour[] gestureInputs;

    [SerializeField] private GameObject instructionsPanel;

    [SerializeField] private TMP_Text statsText;

    [SerializeField] GameObject statsPanel;

    [SerializeField] private GameObject trainingSummaryPopup;

    [SerializeField] private TMP_Text trainingSummaryText;

    [SerializeField] private Button restartTrainingButton;

    private bool hasSavedInitialLayout = false;

    public static AppMode CurrentMode { get; private set; } = AppMode.Instructor;

    [SerializeField]
    Button m_CreateButton;

    private bool isStatsVisible = false;
    private struct SpawnedObjectData
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
    }

    private List<SpawnedObjectData> initialLayout = new();

    public Button createButton
    {
        get => m_CreateButton;
        set => m_CreateButton = value;
    }

    [SerializeField] private Transform m_HoldAnchor; 

    private GameObject heldObject;

    [SerializeField] ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    [SerializeField]
    Button m_PickUpButton;

    public Button pickUpButton
    {
        get => m_PickUpButton;
        set => m_PickUpButton = value;
    }

    [SerializeField]
    Button m_DeleteButton;

    public Button deleteButton
    {
        get => m_DeleteButton;
        set => m_DeleteButton = value;
    }

    [SerializeField]
    GameObject m_ObjectMenu;

    public GameObject objectMenu
    {
        get => m_ObjectMenu;
        set => m_ObjectMenu = value;
    }

    [SerializeField]
    GameObject m_ModalMenu;

    [SerializeField]
    GameObject m_OptionsButton;

    public GameObject modalMenu
    {
        get => m_ModalMenu;
        set => m_ModalMenu = value;
    }

    [SerializeField]
    Button m_SprayButton;

    public Button sprayButton
    {
        get => m_SprayButton;
        set => m_SprayButton = value;
    }

    [SerializeField]
    Animator m_ObjectMenuAnimator;

    public Animator objectMenuAnimator
    {
        get => m_ObjectMenuAnimator;
        set => m_ObjectMenuAnimator = value;
    }

    [SerializeField]
    ObjectSpawner m_ObjectSpawner;

    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField]
    Button m_CancelButton;

    public Button cancelButton
    {
        get => m_CancelButton;
        set => m_CancelButton = value;
    }

    [SerializeField]
    XRInteractionGroup m_InteractionGroup;

    public XRInteractionGroup interactionGroup
    {
        get => m_InteractionGroup;
        set => m_InteractionGroup = value;
    }

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    public ARPlaneManager planeManager
    {
        get => m_PlaneManager;
        set => m_PlaneManager = value;
    }

    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    [SerializeField]
    XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

    public XRInputValueReader<Vector2> dragCurrentPositionInput
    {
        get => m_DragCurrentPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
    }

    bool m_IsPointerOverUI;
    bool m_ShowObjectMenu;
    bool m_ShowOptionsModal;
    Vector2 m_ObjectButtonOffset = Vector2.zero;
    Vector2 m_ObjectMenuOffset = Vector2.zero;
    readonly List<ARFeatheredPlaneMeshVisualizerCompanion> featheredPlaneMeshVisualizerCompanions = new List<ARFeatheredPlaneMeshVisualizerCompanion>();

    void OnEnable()
    {
        m_CreateButton.onClick.AddListener(ShowMenu);
        m_CancelButton.onClick.AddListener(HideMenu);
        m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
        m_PickUpButton.GetComponent<Button>().onClick.AddListener(PickUpFocusedObject);
        m_SprayButton.onClick.AddListener(TriggerSpray);
        m_SprayButton.gameObject.SetActive(false);
        m_PlaneManager.trackablesChanged.AddListener(OnPlaneChanged);
        restartTrainingButton.onClick.AddListener(RestartTrainingFromPopup);
    }
    public void RestartTrainingFromPopup()
    {
        ClearAllObjects();
        TrainingStatsManager.Instance.ResetStats();

        foreach (var item in initialLayout)
        {
            var spawned = Instantiate(item.prefab, item.position, item.rotation, m_ObjectSpawner.transform);

            if (spawned.TryGetComponent<TrackableFire>(out var fire))
            {
                fire.ResetFire();
                TrainingStatsManager.Instance.RegisterFire(fire);
            }
        }
        trainingSummaryPopup.SetActive(false);
        SetTrainingMode();
    }


    void OnDisable()
    {
        m_ShowObjectMenu = false;
        m_CreateButton.onClick.RemoveListener(ShowMenu);
        m_CancelButton.onClick.RemoveListener(HideMenu);
        m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
        m_PlaneManager.trackablesChanged.RemoveListener(OnPlaneChanged);
    }

    void Start()
    {
        HideMenu();
        UpdateModeText();
        ApplyModeSettings();
    }

    void Update()
    {

        if (m_ShowObjectMenu || m_ShowOptionsModal)
        {
            if (!m_IsPointerOverUI && (m_TapStartPositionInput.TryReadValue(out _) || m_DragCurrentPositionInput.TryReadValue(out _)))
            {
                if (m_ShowObjectMenu)
                    HideMenu();

                if (m_ShowOptionsModal)
                    m_ModalMenu.SetActive(false);
            }
        }

        if (CurrentMode == AppMode.Training)
        {
            trainingTimer += Time.deltaTime;

            UpdateTrainingTimerUI();
            if (TrainingStatsManager.Instance.AreAllFiresExtinguished())
            {
                isTrainingModeActive = false;
            }
        }

        UpdateSprayButtonVisibility();
        UpdateDeleteButtonVisibility();
        UpdatePickupButtonVisibility();

        m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
    }

    void UpdateTrainingTimerUI()
    {
        if (trainingTimerText != null)
        {
            int minutes = Mathf.FloorToInt(trainingTimer / 60f);
            int seconds = Mathf.FloorToInt(trainingTimer % 60f);
            trainingTimerText.text = $"Èas: {minutes:D2}:{seconds:D2}";
            trainingTimerText.SetAllDirty();
        }
    }

    void UpdateSprayButtonVisibility()
    {
        if (heldObject != null && heldObject.CompareTag("Pickup") && heldObject.GetComponent<SprayController>() != null)
        {
            m_SprayButton.gameObject.SetActive(true);
        }
        else
        {
            m_SprayButton.gameObject.SetActive(false);
        }
    }
    void UpdatePickupButtonVisibility()
    {
        if (m_ShowObjectMenu)
        {
            m_PickUpButton.gameObject.SetActive(false);
            return;
        }

        var focused = m_InteractionGroup?.focusInteractable;
        if (focused != null && focused.transform.CompareTag("Pickup"))
        {
            m_PickUpButton.gameObject.SetActive(true);
        }
        else
        {
            m_PickUpButton.gameObject.SetActive(false);
        }
    }


    void UpdateDeleteButtonVisibility()
    {
        if (CurrentMode == AppMode.Training)
        {
            m_DeleteButton.gameObject.SetActive(false);
            return;
        }

        if (m_ShowObjectMenu)
        {
            m_DeleteButton.gameObject.SetActive(false);
            return;
        }

        if (heldObject != null)
        {
            m_DeleteButton.gameObject.SetActive(true);
            return;
        }

        var focused = m_InteractionGroup?.focusInteractable;
        if (focused != null)
        {
            var obj = focused.transform.gameObject;
            bool isDeletable = obj.CompareTag("Pickup") || obj.GetComponent<TrackableFire>() != null;
            m_DeleteButton.gameObject.SetActive(isDeletable);
        }
        else
        {
            m_DeleteButton.gameObject.SetActive(false);
        }
    }


    public void SetObjectToSpawn(int objectIndex)
    {
        if (CurrentMode == AppMode.Training)
        {
            return;
        }

        if (m_ObjectSpawner == null)
        {
            return;
        }

        if (m_ObjectSpawner.objectPrefabs.Count > objectIndex)
        {
            m_ObjectSpawner.spawnOptionIndex = objectIndex;
        }
        HideMenu();
    }

    void ShowMenu()
    {
        m_ShowObjectMenu = true;
        m_ObjectMenu.SetActive(true);
        if (!m_ObjectMenuAnimator.GetBool("Show"))
        {
            m_ObjectMenuAnimator.SetBool("Show", true);
        }
    }
    public void ShowHideModal()
    {
        if (m_ModalMenu.activeSelf)
        {
            m_ShowOptionsModal = false;
            m_ModalMenu.SetActive(false);
        }
        else
        {
            m_ShowOptionsModal = true;
            m_ModalMenu.SetActive(true);
        }
    }

public void ClearAllObjects()
{
    foreach (Transform child in m_ObjectSpawner.transform)
    {
        Destroy(child.gameObject);
    }

    TrainingStatsManager.Instance?.ResetStats();
    UpdateStatsText();
}




    public void HideMenu()
    {
        m_ObjectMenuAnimator.SetBool("Show", false);
        m_ShowObjectMenu = false;
    }

    [SerializeField] ARInteractorSpawnTrigger m_SpawnTrigger;


    void ApplyModeRestrictions()
    {
        bool isTraining = CurrentMode == AppMode.Training;


        m_CreateButton.gameObject.SetActive(!isTraining);
        m_ObjectMenu.SetActive(false);
        m_CancelButton.gameObject.SetActive(!isTraining);
        m_DeleteButton.gameObject.SetActive(false);
        m_ModalMenu.SetActive(!isTraining);

        m_OptionsButton.SetActive(!isTraining);
        if (m_SpawnTrigger != null)
            m_SpawnTrigger.enabled = !isTraining;

        foreach (var door in GameObject.FindGameObjectsWithTag("ExitDoor"))
        {
            var grabbable = door.GetComponent<XRGrabInteractable>();
            if (grabbable != null)
                grabbable.enabled = !isTraining;
        }

        foreach (var gesture in gestureInputs)
        {
            if (gesture == null) continue;

            if (isTraining && (gesture is ScreenSpaceRotateInput
                            || gesture is ScreenSpacePinchScaleInput))
            {
                gesture.enabled = false;
            }
            else
            {
                gesture.enabled = true;
            }
        }

        if (heldObject != null && isTraining && !heldObject.CompareTag("Pickup"))
        {
            if (heldObject.TryGetComponent<SprayController>(out var spray))
                spray.StopSpray();

            heldObject.transform.SetParent(null);
            heldObject = null;
            m_SprayButton.gameObject.SetActive(false);
        }
    }
    public void ApplyModeSettings()
    {
        bool isInstructor = CurrentMode == AppMode.Instructor;

        if (isInstructor)
        {
            selectInput.enabled = true;
            rotateInput.enabled = true;
            scaleInput.enabled = true;
        }
        else
        {

            selectInput.enabled = true;
            rotateInput.enabled = false;
            scaleInput.enabled = false;
        }

        if (m_InteractionGroup != null)
            m_InteractionGroup.enabled = true;

        if (m_RayInteractor != null)
            m_RayInteractor.enabled = true;

        foreach (var gesture in gestureInputs)
        {
            if (gesture == null) continue;

            if (gesture is ScreenSpaceRotateInput || gesture is ScreenSpacePinchScaleInput)
            {
                gesture.enabled = CurrentMode == AppMode.Instructor;
            }
            else
            {
                gesture.enabled = true;
            }
        }
    }



    void PickUpFocusedObject()
    {
        if (heldObject != null)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            if (m_RaycastManager.Raycast(screenCenter, s_Hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;

                heldObject.transform.SetParent(null);
                heldObject.transform.position = hitPose.position;
                heldObject.transform.rotation = hitPose.rotation;

                if (heldObject.TryGetComponent<SprayController>(out var spray))
                {
                    spray.StopSpray();
                }

                heldObject = null;
                m_SprayButton.gameObject.SetActive(false);
            }
            return;
        }

        if (m_InteractionGroup.focusInteractable != null)
        {

            GameObject target = m_InteractionGroup.focusInteractable.transform.gameObject;

            if (CurrentMode == AppMode.Training && !target.CompareTag("Pickup"))
            {
                return;
            }

            if (target.CompareTag("Pickup"))
            {

                heldObject = target;
                heldObject.transform.SetParent(m_HoldAnchor);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
                m_SprayButton.gameObject.SetActive(true);
            }
        }
    }

    public void TriggerSpray()
    {
        if (heldObject == null || !heldObject.CompareTag("Pickup")) return;

        if (heldObject.TryGetComponent<SprayController>(out var spray))
        {
            if (spray.isSpraying)
                spray.StopSpray();
            else
                spray.StartSpray();
        }
    }

    void DeleteFocusedObject()
    {
        var focused = m_InteractionGroup?.focusInteractable;
        if (focused != null)
        {
            var obj = focused.transform.gameObject;

            if (heldObject == obj)
            {
                return;
            }

            if (obj.TryGetComponent<TrackableFire>(out var fire))
            {
                fire.HasMistakeLogged = false;
                TrainingStatsManager.Instance?.UnregisterFire(fire);
            }

            Destroy(obj);
            UpdateStatsText();
        }
    }

    public void SetInstructorMode()
    {
        if (m_SpawnTrigger != null)
            m_SpawnTrigger.enabled = true;

        TrainingStatsManager.Instance?.ResetStats();
        savedPeopleCount = 0;

        CurrentMode = AppMode.Instructor;

        trainingTimer = 0f;

        if (trainingTimerText != null)
            trainingTimerText.gameObject.SetActive(false);
        if (timerPanel != null)
            timerPanel.SetActive(false);

        UpdateModeText();

        m_CreateButton.gameObject.SetActive(true);
        m_DeleteButton.gameObject.SetActive(true);
        m_CancelButton.gameObject.SetActive(true);
        m_ModalMenu.SetActive(true);

        ApplyModeRestrictions();
        ApplyModeSettings();
        Object.FindFirstObjectByType<InstructionUI>()?.RefreshInstructions();
        FindAnyObjectByType<InstructionUI>()?.HideInstructions();

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

    }

    public void SetTrainingMode()
    {

        if (m_SpawnTrigger != null)
            m_SpawnTrigger.enabled = false;

        CurrentMode = AppMode.Training;
        UpdateModeText();

        trainingTimer = 0f;

        if (timerPanel != null)
            timerPanel.SetActive(true);

        isTrainingModeActive = true;

        if (trainingTimerText != null)
            trainingTimerText.gameObject.SetActive(true);

        m_CreateButton.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);
        m_CancelButton.gameObject.SetActive(false);
        m_ObjectMenu.SetActive(false);
        m_ModalMenu.SetActive(false);

        if (heldObject != null && !heldObject.CompareTag("Pickup"))
        {
            heldObject = null;
        }

        if (!hasSavedInitialLayout)
        {
            initialLayout.Clear();

            foreach (Transform child in m_ObjectSpawner.transform)
            {
                GameObject obj = child.gameObject;

                if (obj.TryGetComponent<TrackableFire>(out _) && obj.TryGetComponent<RuntimeSpawnInfo>(out var info))
                {
                    var data = new SpawnedObjectData
                    {
                        prefab = info.prefabReference,
                        position = obj.transform.position,
                        rotation = obj.transform.rotation
                    };
                    initialLayout.Add(data);
                }
            }

            hasSavedInitialLayout = true;
        }


        var stats = TrainingStatsManager.Instance;
        if (stats != null)
            stats.BeginTrainingSession();

        ApplyModeRestrictions();
        ApplyModeSettings();
        selectInput.enabled = true;
        FindAnyObjectByType<InstructionUI>()?.HideInstructions();

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }


    public void ToggleAppMode()
    {
        CurrentMode = (CurrentMode == AppMode.Instructor) ? AppMode.Training : AppMode.Instructor;
        UpdateModeText();

        if (CurrentMode == AppMode.Training)
        {
            objectSpawner.spawnOptionIndex = -1; 
        }

        m_CreateButton.gameObject.SetActive(CurrentMode == AppMode.Instructor);
        m_DeleteButton.gameObject.SetActive(CurrentMode == AppMode.Instructor);
        m_CancelButton.gameObject.SetActive(false);
        m_ObjectMenu.SetActive(false);
        m_ModalMenu.SetActive(false);
        ApplyModeRestrictions();

        if (CurrentMode == AppMode.Instructor)
        {
            SetInstructorMode();
        }
        else
        {
            SetTrainingMode();
        }

    }

    void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        if (eventArgs.added.Count > 0)
        {
            foreach (var plane in eventArgs.added)
            {
                if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                }
            }
        }

        if (eventArgs.removed.Count > 0)
        {
            foreach (var plane in eventArgs.removed)
            {
                if (plane.Value != null && plane.Value.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                    featheredPlaneMeshVisualizerCompanions.Remove(visualizer);
            }
        }

        if (m_PlaneManager.trackables.count != featheredPlaneMeshVisualizerCompanions.Count)
        {
            featheredPlaneMeshVisualizerCompanions.Clear();
            foreach (var trackable in m_PlaneManager.trackables)
            {
                if (trackable.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                }
            }
        }
    }

    public void ResetTrainingStats()
    {
        TrainingStatsManager.Instance.ResetStats();
    }
    public void ToggleStatsUI()
    {
        isStatsVisible = !isStatsVisible;

        if (statsPanel != null)
            statsPanel.SetActive(isStatsVisible);

        if (isStatsVisible)
        {
            UpdateStatsText();
        }
    }

    public void UpdateStatsText()
    {
        if (statsText != null && TrainingStatsManager.Instance != null)
        {
            statsText.text = TrainingStatsManager.Instance.GenerateSummary();
        }
    }

    public void StopTraining()
    {
        isTrainingModeActive = false;

        if (timerPanel != null)
            timerPanel.SetActive(false);

        if (trainingTimerText != null)
            trainingTimerText.gameObject.SetActive(false);

        if (trainingSummaryPopup != null)
        {
            ShowTrainingSummary();
        }
    }

    public void CloseSummaryAndGoInstructor()
    {
        trainingSummaryPopup.SetActive(false);
        ClearAllObjects();
        SetInstructorMode();
    }

    public void SavePerson()
    {
        savedPeopleCount++;
    }

    public void ShowTrainingSummary()
    {
        trainingSummaryPopup.SetActive(true);

        string summary = TrainingStatsManager.Instance.GenerateSummary();
        int minutes = Mathf.FloorToInt(trainingTimer / 60f);
        int seconds = Mathf.FloorToInt(trainingTimer % 60f);
        string formattedTime = $"Èas simulácie: {minutes:D2}:{seconds:D2}";

        string peopleSavedInfo = $"Zachránení ¾udia: {savedPeopleCount}";

        trainingSummaryText.text = $"{peopleSavedInfo}\n{summary}\n\n{formattedTime}";
    }


}