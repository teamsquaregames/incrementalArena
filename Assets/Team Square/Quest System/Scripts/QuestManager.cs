using System;
using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.UI;

public class QuestManager : Singleton<QuestManager>
{
    public Action<Quest> OnQuestProgressUpdated;
    public Action<Quest> OnQuestCompleted;
    public Action OnQuestCompletedTutorial;
    public Action<Quest> OnQuestStarted;
    public Action OnAllQuestsCompleted;
    
    [Header("Quest Configuration")]
    [Tooltip("Quest assets in sequential order (only used if useQuestAssets is true)")]
    [InlineEditor]
    [SerializeField] private QuestData[] questAssets;
    
    private Quest currentQuest;
    private List<Quest> questSequence = new List<Quest>();
    
    public Quest CurrentQuest => currentQuest;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        BuildQuestSequence();
        
        GameData.Instance.OnTrackedValueChanged -= OnTrackedValueChanged;
        GameData.Instance.OnTrackedValueChanged += OnTrackedValueChanged;
        
        if (questSequence.Count > 0)
            LoadQuestProgress();
    }
    
    private void BuildQuestSequence()
    {
        questSequence.Clear();
        foreach (var questAsset in questAssets)
        {
            if (questAsset != null)
            {
                Quest quest = questAsset.ToQuest();
                
                if (GameData.Instance.completedQuestIds.Contains(quest.questId))
                {
                    quest.MarkComplete();
                }
                
                questSequence.Add(quest);
            }
        }
        // Debug.Log($"QuestManager: Loaded {questSequence.Count} quests from ScriptableObject assets");
    }
    
    private void OnDestroy()
    {
        GameData.Instance.OnTrackedValueChanged -= OnTrackedValueChanged;
    }
    
    private void OnTrackedValueChanged(TrackedValueType type, double value)
    {
        if (currentQuest != null && !currentQuest.isComplete)
        {
            bool isRelevant = false;
            foreach (var objective in currentQuest.objectives)
            {
                if (objective.trackedValueType == type)
                {
                    isRelevant = true;
                    break;
                }
            }
            
            if (isRelevant)
            {
                CheckCurrentQuestProgress();
            }
        }
    }
    
    private void StartQuest(int index)
    {
        if (index < 0 || index >= questSequence.Count) return;
        
        currentQuest = questSequence[index];
        
        for (int i = 0; i < currentQuest.objectives.Count; i++)
        {
            currentQuest.objectives[i].Initialize(currentQuest.questId, i);
        }
        
        GameData.Instance.currentQuestIndex = index;
        GameData.Instance.Save();
        
        OnQuestStarted?.Invoke(currentQuest);
        CheckCurrentQuestProgress();
    }
    
    private void CheckCurrentQuestProgress()
    {
        if (currentQuest == null) return;
        
        OnQuestProgressUpdated?.Invoke(currentQuest);
        
        if (currentQuest.CheckCompletion())
        {
            CompleteCurrentQuest();
        }
    }
    
    private void CompleteCurrentQuest()
    {
        if (currentQuest == null || currentQuest.isComplete)
            return;
        
        currentQuest.MarkComplete();
        
        if (!GameData.Instance.completedQuestIds.Contains(currentQuest.questId))
        {
            GameData.Instance.completedQuestIds.Add(currentQuest.questId);
        }
        
        OnQuestCompleted?.Invoke(currentQuest);
        OnQuestCompletedTutorial?.Invoke();
        
        int nextQuestIndex = GameData.Instance.currentQuestIndex + 1;
        
        if (nextQuestIndex < questSequence.Count)
        {
            StartQuest(nextQuestIndex);
        }
        else
        {
            currentQuest = null;
            OnAllQuestsCompleted?.Invoke();
        }
        
        GameData.Instance.Save();
    }
    
    public void LoadQuestProgress()
    {
        int savedIndex = GameData.Instance.currentQuestIndex;
        
        if (savedIndex < 0 || savedIndex >= questSequence.Count)
            savedIndex = 0;
        
        while (savedIndex < questSequence.Count && questSequence[savedIndex].isComplete)
            savedIndex++;
        
        if (savedIndex < questSequence.Count)
        {
            StartQuest(savedIndex);
        }
        else
        {
            currentQuest = null;
            GameData.Instance.currentQuestIndex = questSequence.Count;
            OnAllQuestsCompleted?.Invoke();
        }
    }
        
    
    #region Public API
    
    public bool AreAllQuestsCompleted()
    {
        return GameData.Instance.currentQuestIndex >= questSequence.Count || 
               (currentQuest != null && currentQuest.isComplete && GameData.Instance.currentQuestIndex == questSequence.Count - 1);
    }
    
    public void AddQuestToSequence(Quest quest)
    {
        questSequence.Add(quest);
        
        if (currentQuest == null && questSequence.Count == 1)
        {
            StartQuest(0);
        }
    }
    
    public void ResetQuestSequence()
    {
        foreach (var quest in questSequence)
        {
            quest.isComplete = false;
        }
        
        GameData.Instance.currentQuestIndex = 0;
        
        GameData.Instance.currentQuestIndex = 0;
        GameData.Instance.completedQuestIds.Clear();
        GameData.Instance.ClearQuestObjectiveSnapshots();
        GameData.Instance.Save();
        
        if (questSequence.Count > 0)
            StartQuest(0);
        else
            currentQuest = null;
    }
    
    #endregion
    
    [Button]
    public void DebugCompleteCurrentQuest()
    {
        if (currentQuest != null && !currentQuest.isComplete)
        {
            CompleteCurrentQuest();
        }
    }
}