using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public struct Goal
{
    public GoalManager.OnboardingGoals CurrentGoal;


    public bool Completed;

    public Goal(GoalManager.OnboardingGoals goal)
    {
        CurrentGoal = goal;
        Completed = false;
    }
}

public class GoalManager : MonoBehaviour
{
    public enum OnboardingGoals
    {
        Empty,

        FindSurfaces,

        TapSurface,

    }

    [Serializable]
    public class Step
    {
        [SerializeField]
        public GameObject stepObject;

        [SerializeField]
        public string buttonText;

        [SerializeField]
        public bool includeSkipButton;
    }

    [SerializeField]
    List<Step> m_StepList = new List<Step>();

    public List<Step> stepList
    {
        get => m_StepList;
        set => m_StepList = value;
    }

    [SerializeField]
    ObjectSpawner m_ObjectSpawner;

    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField]
    GameObject m_GreetingPrompt;

    public GameObject greetingPrompt
    {
        get => m_GreetingPrompt;
        set => m_GreetingPrompt = value;
    }

    [SerializeField]
    GameObject m_OptionsButton;

    public GameObject optionsButton
    {
        get => m_OptionsButton;
        set => m_OptionsButton = value;
    }

    [SerializeField]
    GameObject m_CreateButton;

    public GameObject createButton
    {
        get => m_CreateButton;
        set => m_CreateButton = value;
    }

    [SerializeField]
    ARTemplateMenuManager m_MenuManager;

    public ARTemplateMenuManager menuManager
    {
        get => m_MenuManager;
        set => m_MenuManager = value;
    }

    const int k_NumberOfSurfacesTappedToCompleteGoal = 1;

    Queue<Goal> m_OnboardingGoals;
    Coroutine m_CurrentCoroutine;
    Goal m_CurrentGoal;
    bool m_AllGoalsFinished;
    int m_SurfacesTapped;
    int m_CurrentGoalIndex = 0;

    void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame && !m_AllGoalsFinished && (m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces))
        {
            if (m_CurrentCoroutine != null)
            {
                StopCoroutine(m_CurrentCoroutine);
            }
            CompleteGoal();
        }
    }

    void CompleteGoal()
    {
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
            m_ObjectSpawner.objectSpawned -= OnObjectSpawned;

        m_CurrentGoal.Completed = true;
        m_CurrentGoalIndex++;
        if (m_OnboardingGoals.Count > 0)
        {
            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            m_StepList[m_CurrentGoalIndex].stepObject.SetActive(true);
        }
        else
        {
            m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            m_AllGoalsFinished = true;
            return;
        }

        PreprocessGoal();
    }

    void PreprocessGoal()
    {
        switch (m_CurrentGoal.CurrentGoal)
        {
            case OnboardingGoals.FindSurfaces:
                Invoke(nameof(CompleteGoal), 5f);
                break;
            case OnboardingGoals.TapSurface:
                m_SurfacesTapped = 0;
                m_ObjectSpawner.objectSpawned += OnObjectSpawned;
                break;
            default:
                break;
        }
    }


    public void ForceCompleteGoal()
    {
        CompleteGoal();
    }

    void OnObjectSpawned(GameObject spawnedObject)
    {
        m_SurfacesTapped++;
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface && m_SurfacesTapped >= k_NumberOfSurfacesTappedToCompleteGoal)
        {
            CompleteGoal();
        }
    }

    public void StartCoaching()
    {
        if (m_OnboardingGoals != null)
        {
            m_OnboardingGoals.Clear();
        }

        m_OnboardingGoals = new Queue<Goal>();

        if (!m_AllGoalsFinished)
        {
            var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
            m_OnboardingGoals.Enqueue(findSurfaceGoal);
        }

        int startingStep = m_AllGoalsFinished ? 1 : 0;

        var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);

        m_OnboardingGoals.Enqueue(tapSurfaceGoal);

        m_CurrentGoal = m_OnboardingGoals.Dequeue();
        m_AllGoalsFinished = false;
        m_CurrentGoalIndex = startingStep;

        m_GreetingPrompt.SetActive(false);
        m_OptionsButton.SetActive(true);
        m_CreateButton.SetActive(true);
        m_MenuManager.enabled = true;

        for (int i = startingStep; i < m_StepList.Count; i++)
        {
            if (i == startingStep)
            {
                m_StepList[i].stepObject.SetActive(true);
                PreprocessGoal();
            }
            else
            {
                m_StepList[i].stepObject.SetActive(false);
            }
        }

    }
}