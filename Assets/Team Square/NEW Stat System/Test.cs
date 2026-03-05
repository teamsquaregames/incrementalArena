using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private StatModule m_statModule;
    [SerializeField] private List<Stats.StatModifier> m_modifiers;

    [Button]
    public void PrintStats()
    {
        print(m_statModule.GetValue(Stats.StatType.AttackSpeed));
        print(m_statModule.GetValue(Stats.StatType.AttackDamage));
    }

    [Button]
    public void ApplyModifiers()
    {
        foreach (var mod in m_modifiers)
            m_statModule.AddModifier(mod);
    }

    [Button]
    public void RemoveModifiers()
    {
        foreach (var mod in m_modifiers)
            m_statModule.RemoveModifier(mod);
    }
}