using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Quest
{
    [Header("Quest Info")]
    [Tooltip("Unique identifier for this quest")]
    public string questId;
    
    [Tooltip("Display name of the quest")]
    public string questName;
    
    [Tooltip("Description shown to the player")]
    [TextArea(3, 5)]
    public string description;
    
    [Tooltip("Icon sprite for this quest")]
    public Sprite questIcon;
    
    [Header("Objectives")]
    [Tooltip("List of objectives that must all be completed")]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [Header("State")]
    [Tooltip("Has this quest been completed?")]
    public bool isComplete = false;
    
    [Header("Rewards")]
    [Tooltip("Stars awarded upon completion")]
    public double rewardStars = 0;
    
    public STNodeAsset linkedNode;
    
    public bool CheckCompletion()
    {
        if (isComplete) return false;
        
        foreach (var objective in objectives)
        {
            if (!objective.IsComplete())
                return false;
        }
        
        return true;
    }
    
    public float GetOverallProgress(GameData gameData)
    {
        if (objectives.Count == 0) return 0f;
        
        float totalProgress = 0f;
        foreach (var objective in objectives)
        {
            totalProgress += objective.GetNormalizedProgress();
        }
        
        return totalProgress / objectives.Count;
    }
    
    public void MarkComplete()
    {
        isComplete = true;
    }
}