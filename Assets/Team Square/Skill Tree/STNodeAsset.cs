using Sirenix.OdinInspector;
using System;
using Stats;
using UnityEngine;

[CreateAssetMenu(fileName = "TTN_", menuName = "TT Node Asset")]
[Serializable]
public class STNodeAsset : ScriptableObject
{
    [TitleGroup("Settings")]
    [HorizontalGroup("Settings/Row", Width = 300)]
    [VerticalGroup("Settings/Row/Left")]
    [SerializeField] protected string m_displayName;

    [VerticalGroup("Settings/Row/Left")]
    [SerializeField] private string m_id;

    [VerticalGroup("Settings/Row/Left")]
    [SerializeField] private int m_maxLevel;

    [VerticalGroup("Settings/Row/Left")]
    [SerializeField] protected NodeRank m_rank;

    [VerticalGroup("Settings/Row/Right")]
    [HideLabel]
    [PreviewField(90, ObjectFieldAlignment.Center)]
    [SerializeField] protected Sprite m_icon;

    [TitleGroup("Settings")]
    [SerializeField, TextArea(5, 10)] protected string m_description;

    [TitleGroup("Costs")]
    [SerializeField] protected Cost[] m_cost;

    [TitleGroup("Bonuses")]
    [SerializeField] protected LeveledStatModifier[] m_statModifiers;

    private int m_lastMaxLevel = -1;

    [OnInspectorGUI]
    private void CheckMaxLevelChanged()
    {
        if (m_maxLevel == m_lastMaxLevel) return;
        m_lastMaxLevel = m_maxLevel;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            OnMaxLevelChanged();
            UnityEditor.EditorUtility.SetDirty(this);
        };
    }

    #region Getters
    public LeveledStatModifier[] StatModifiers => m_statModifiers;
    public string DisplayName => m_displayName;
    public string ID => m_id;
    public Sprite Icon => m_icon;
    public NodeRank Rank => m_rank;
    public string Description => m_description;
    public Cost[] Cost => m_cost;
    public int MaxLevel => m_maxLevel;
    #endregion

    private void OnMaxLevelChanged()
    {
        if (m_statModifiers == null) return;

        foreach (var modifier in m_statModifiers)
        {
            if (modifier == null) continue;
            modifier.ResizeValues(m_maxLevel);
        }
    }
}