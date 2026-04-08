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
    [EnumToggleButtons]
    [SerializeField] private LevelingType m_levelingType = LevelingType.Limited;

    [VerticalGroup("Settings/Row/Left")]
    [ShowIf(nameof(IsLimited))]
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

    private bool IsLimited => m_levelingType == LevelingType.Limited;

    [OnInspectorGUI]
    private void CheckMaxLevelChanged()
    {
#if UNITY_EDITOR
        if (!IsLimited) return;
        if (m_maxLevel == m_lastMaxLevel) return;
        m_lastMaxLevel = m_maxLevel;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            OnMaxLevelChanged();
            UnityEditor.EditorUtility.SetDirty(this);
        };
#endif
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
    public LevelingType LevelingType => m_levelingType;
    public bool IsUnlimited => m_levelingType == LevelingType.Unlimited;
    #endregion

    private void OnMaxLevelChanged()
    {
        // resize amount array of each cost to fit the new max level
        if (m_cost != null)
        {
            for (int i = 0; i < m_cost.Length; i++)
            {
                Cost cost = m_cost[i];
                cost.ResizeAmounts(m_maxLevel);
                m_cost[i] = cost;
            }
        }

        if (m_statModifiers == null) return;

        foreach (var modifier in m_statModifiers)
        {
            if (modifier == null) continue;
            modifier.ResizeValues(m_maxLevel);
        }
    }

#if UNITY_EDITOR
    private string m_previousId;

    private void OnValidate()
    {
        if (m_id != m_previousId)
            OnAssetChanged();
    }

    [Button]
    private void OnAssetChanged()
    {
        ///. change le nom du fichier pour correspondre à l'ID
        if (string.IsNullOrEmpty(m_id)) return;
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (assetName != m_id)
        {
            UnityEditor.AssetDatabase.RenameAsset(assetPath, m_id);
            UnityEditor.EditorUtility.SetDirty(this);
        }

    }
#endif
}