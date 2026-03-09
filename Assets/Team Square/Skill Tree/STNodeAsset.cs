using Sirenix.OdinInspector;
using System;
using Stats;
using UnityEngine;
using Utils;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "TTN_", menuName = "TT Node Asset")]
[Serializable]
public class STNodeAsset : ScriptableObject
{
    [TitleGroup("Settings")]
    [SerializeField] protected string m_displayName;
    [SerializeField] private string m_id;
    [SerializeField] private int m_maxLevel;

    [PreviewField(ObjectFieldAlignment.Center, Height = 100f)]
    [SerializeField] protected Sprite m_icon;
    [SerializeField] protected NodeRank m_rank;
    [SerializeField, TextArea(5, 10)] protected string m_description;

    [TitleGroup("Costs")]
    [SerializeField] protected Cost[] m_cost;

    [TitleGroup("Bonuses")]
    [SerializeField] private bool m_freeBuildings;
    [SerializeField] protected Cost[] m_currencies;
    [SerializeField] protected LeveledStatModifier[] m_statModifiers;

    #region Gettters
    public LeveledStatModifier[] StatModifiers => m_statModifiers;
    public string DisplayName => m_displayName;
    public string ID => m_id;
    public Sprite Icon => m_icon;
    public NodeRank Rank => m_rank;
    public string Description => m_description;
    public Cost[] Cost => m_cost;
    public bool FreeBuildings => m_freeBuildings;
    public Cost[] Currencies => m_currencies;
    public int MaxLevel => m_maxLevel;
    #endregion

    #region Helper
    
    [TitleGroup("Cost")]
    [HorizontalGroup("Array", LabelWidth = 80)]
    [SerializeField, Min(1)] protected int m_length = 5;
    [TitleGroup("Cost")]
    [HorizontalGroup("Array")]
    [SerializeField] protected int m_offset = 0;

    [Space]
    [SerializeField] protected CurrencyAsset m_previewCurrency;

    [HorizontalGroup("Currencies", LabelWidth = 80), Space]
    [SerializeField] protected float m_pricesBase = 10;
    [HorizontalGroup("Currencies"), Space]
    [SerializeField] protected float m_pricesLinear = 1;
    [HorizontalGroup("Currencies"), Space]
    [SerializeField] protected float m_pricesExpo = 1;

    [TitleGroup("Cost")]
    [ReadOnly, SerializeField] protected Cost[] m_testCosts = new Cost[0];

    [OnInspectorGUI]
    private void Preview()
    {
        // Update costs preview
        if (m_cost != null && m_cost.Length > 0)
        {
            if (m_testCosts.Length != m_cost.Length)
            {
                m_testCosts = new Cost[m_cost.Length];
            }

            for (int ci = 0; ci < m_cost.Length; ci++)
            {
                m_testCosts[ci].currencyAsset = m_cost[ci].currencyAsset;

                ulong[] amounts = new ulong[m_length];
                for (int i = m_offset; i < m_length; i++)
                {
                    double price = math.round((m_pricesLinear * (i - m_offset)) + m_pricesBase * math.pow(m_pricesExpo, i - m_offset));
                    amounts[i] = (ulong)math.clamp(price, 0, (double)ulong.MaxValue);
                }
                m_testCosts[ci].SetAmounts(amounts);
            }
        }
        else
        {
            m_testCosts = new Cost[0];
        }
    }

    [Button("Apply Generated Values to Costs")]
    protected void SetValues()
    {
        // Populate costs amounts arrays from computed prices
        if (m_cost != null && m_cost.Length > 0)
        {
            int appliedCount = 0;

            for (int ci = 0; ci < m_cost.Length; ci++)
            {
                // Skip if a preview currency is selected and this isn't it
                if (m_previewCurrency != null && m_cost[ci].currencyAsset != m_previewCurrency)
                    continue;

                ulong[] amounts = new ulong[m_length];
                for (int i = m_offset; i < m_length; i++)
                {
                    double price = math.round((m_pricesLinear * (i - m_offset)) + m_pricesBase * math.pow(m_pricesExpo, i - m_offset));
                    amounts[i] = (ulong)math.clamp(price, 0, (double)ulong.MaxValue);
                }
                m_cost[ci].SetAmounts(amounts);
                appliedCount++;
            }

            if (appliedCount > 0)
            {
                if (m_previewCurrency != null)
                    this.Log($"Generated costs for {m_previewCurrency.name} with {m_length} levels");
                else
                    this.Log($"Generated costs for {appliedCount} currencies with {m_length} levels");
            }
            else
            {
                this.LogWarning($"No costs found for currency {m_previewCurrency?.name}");
            }
        }
        else
        {
            this.LogWarning("No costs configured. Add currencies to the costs array first.");
        }
    }
    #endregion
}