using System;
using UnityEngine;

[Serializable]
public class QuestObjective
{
    [Tooltip("The tracked value type this objective monitors")]
    public TrackedValueType trackedValueType;
    
    [Tooltip("The target value needed to complete this objective (progress from quest start, not lifetime)")]
    public double targetValue;
    
    [NonSerialized] private string snapshotKey;
    
    public void Initialize(string questId, int objectiveIndex)
    {
        snapshotKey = $"{questId}_obj_{objectiveIndex}";
        
        double existingSnapshot = GameData.Instance.GetQuestObjectiveSnapshot(snapshotKey);
        if (existingSnapshot == 0 && GameData.Instance.GetTrackedValue(trackedValueType) == 0)
        {
            if (!GameData.Instance.questObjectiveSnapshots.ContainsKey(snapshotKey))
            {
                double currentValue = GameData.Instance.GetTrackedValue(trackedValueType);
                GameData.Instance.SetQuestObjectiveSnapshot(snapshotKey, currentValue);
            }
        }
        else if (!GameData.Instance.questObjectiveSnapshots.ContainsKey(snapshotKey))
        {
            double currentValue = GameData.Instance.GetTrackedValue(trackedValueType);
            GameData.Instance.SetQuestObjectiveSnapshot(snapshotKey, currentValue);
        }
    }
    
    public bool IsComplete()
    {
        return GetProgress() >= targetValue;
    }
    
    public double GetProgress()
    {
        if (string.IsNullOrEmpty(snapshotKey))
        {
            // Debug.LogWarning($"QuestObjective for {trackedValueType} has not been initialized. Call Initialize() first.");
            return 0;
        }
        
        double currentValue = GameData.Instance.GetTrackedValue(trackedValueType);
        double startValue = GameData.Instance.GetQuestObjectiveSnapshot(snapshotKey);
        
        return currentValue - startValue;
    }
    
    public float GetNormalizedProgress()
    {
        if (targetValue <= 0) return 0f;
        return Mathf.Clamp01((float)(GetProgress() / targetValue));
    }
    
    public void ResetSnapshot(string questId, int objectiveIndex)
    {
        snapshotKey = $"{questId}_obj_{objectiveIndex}";
        double currentValue = GameData.Instance.GetTrackedValue(trackedValueType);
        GameData.Instance.SetQuestObjectiveSnapshot(snapshotKey, currentValue);
    }
}