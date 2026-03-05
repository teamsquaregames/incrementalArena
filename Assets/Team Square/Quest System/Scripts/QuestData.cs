using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// ScriptableObject version of Quest for easier management in the Unity Editor.
/// Allows you to create quest assets that can be easily configured and reused.
/// 
/// To create: Right-click in Project > Create > Quests > Quest Data
/// </summary>
[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data", order = 1)]
public class QuestData : ScriptableObject
{
    public string questId;
    [TextArea(3, 5)] public string description;
    [PreviewField(100)] public Sprite questIcon; public QuestObjective[] objectives;
    public int rewardStars = 0;
    public STNodeAsset linkedNode;
    
    public Quest ToQuest()
    {
        Quest quest = new Quest
        {
            questId = this.questId,
            description = this.description,
            questIcon = this.questIcon,
            rewardStars = this.rewardStars,
            linkedNode = this.linkedNode,
            isComplete = false
        };
        
        // Copy objectives
        quest.objectives.Clear();
        if (objectives != null)
        {
            foreach (var obj in objectives)
            {
                quest.objectives.Add(new QuestObjective
                {
                    trackedValueType = obj.trackedValueType,
                    targetValue = obj.targetValue,
                });
            }
        }
        
        return quest;
    }
}