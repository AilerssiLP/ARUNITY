using System.Collections.Generic;
using UnityEngine;

public class TrainingStatsManager : MonoBehaviour
{
    public static TrainingStatsManager Instance;

    private Dictionary<TrackableFire.FireType, List<TrackableFire>> fires = new();
    private Dictionary<TrackableFire.FireType, int> correctExtinguishes = new();
    private List<string> mistakeLogs = new();
    private ARTemplateMenuManager menuManager;
    private Dictionary<TrackableFire.FireType, int> mistakeCounts = new();
    private Dictionary<string, int> detailedMistakeCounts = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void BeginTrainingSession()
    {}

    public bool AreAllFiresExtinguished()
    {
        foreach (var type in fires.Keys)
        {
            foreach (var fire in fires[type])
            {
                if (!fire.IsExtinguished)
                    return false;
            }
        }
        return true;
    }

    public void RegisterFire(TrackableFire fire)
    {
        if (!fires.ContainsKey(fire.fireType))
        {
            fires[fire.fireType] = new List<TrackableFire>();
            correctExtinguishes[fire.fireType] = 0;
        }

        fires[fire.fireType].Add(fire);
    }

    public static void ReportExtinguished(TrackableFire fire, bool correct, TrackableFire.FireType usedExtinguisherType)
    {
        if (correct)
        {
            Instance.correctExtinguishes[fire.fireType]++;
        }
        else
        {
            if (!Instance.mistakeCounts.ContainsKey(fire.fireType))
                Instance.mistakeCounts[fire.fireType] = 0;

            Instance.mistakeCounts[fire.fireType]++;

            string key = $"{fire.fireType} <- used {usedExtinguisherType}";
            if (!Instance.detailedMistakeCounts.ContainsKey(key))
                Instance.detailedMistakeCounts[key] = 0;

            Instance.detailedMistakeCounts[key]++;

            if (!fire.HasMistakeLogged)
            {
                Instance.mistakeLogs.Add($"Nesprávny hasiaci prístroj: použitý {usedExtinguisherType} na {fire.fireType}");
                fire.HasMistakeLogged = true;
            }
        }
    }



    public void PrintStats()
    {
        Debug.Log("Training Summary:");

        foreach (var type in fires.Keys)
        {
            int total = fires[type].Count;
            int correct = correctExtinguishes[type];
            Debug.Log($"• {type}: {correct}/{total} extinguished");
        }

        if (mistakeLogs.Count > 0)
        {
            Debug.Log("Mistakes:");
            foreach (var m in mistakeLogs)
                Debug.Log("   - " + m);
        }
    }

    public void ResetStats()
    {
        fires.Clear();
        correctExtinguishes.Clear();
        mistakeCounts.Clear();
        detailedMistakeCounts.Clear();

        if (menuManager == null)
            menuManager = FindAnyObjectByType<ARTemplateMenuManager>();

        menuManager?.UpdateStatsText();
    }


    public string GenerateSummary()
    {
        System.Text.StringBuilder sb = new();

        bool hasAnyFires = false;

        foreach (var type in fires.Keys)
        {
            if (fires[type].Count == 0)
                continue;

            hasAnyFires = true;
            int total = fires[type].Count;
            int correct = correctExtinguishes[type];
            sb.AppendLine($"• {type}: {correct}/{total} uhasených");
        }

        if (!hasAnyFires)
        {
            sb.AppendLine("Zatia¾ nie sú žiadne sledované objekty.");
        }

        if (mistakeCounts.Count > 0)
        {
            sb.AppendLine("\nChyby pod¾a typu požiaru:");
            foreach (var type in mistakeCounts)
            {
                sb.AppendLine($" - {type.Key}: {type.Value}× chybných pokusov");
            }
        }

        if (detailedMistakeCounts.Count > 0)
        {
            sb.AppendLine("\nDetaily chýb:");
            foreach (var pair in detailedMistakeCounts)
            {
                sb.AppendLine($" - {pair.Key}: {pair.Value}×");
            }
        }

        return sb.ToString();
    }
    public void UnregisterFire(TrackableFire fire)
    {
        if (fires.ContainsKey(fire.fireType))
        {
            fires[fire.fireType].Remove(fire);
            if (fires[fire.fireType].Count == 0)
            {
                fires.Remove(fire.fireType);
                correctExtinguishes.Remove(fire.fireType);
            }
        }
    }

}
