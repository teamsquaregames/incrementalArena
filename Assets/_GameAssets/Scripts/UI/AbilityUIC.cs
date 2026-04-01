using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.UI;

public class AbilityUIC : UIContainer
{
    [TitleGroup("References")]
    [SerializeField] private List<AbilityDisplay> m_slots = new List<AbilityDisplay>();

    private readonly Dictionary<AbilityConfig, AbilityDisplay> m_assigned = new();

    public override void Init()
    {
        foreach (AbilityDisplay abilityDisplay in m_slots)
        {
            abilityDisplay.gameObject.SetActive(false);
        }
    }

    public void AddDisplay(AbilityConfig config)
    {
        if (config == null || m_assigned.ContainsKey(config)) return;

        AbilityDisplay slot = m_slots.Find(s => !s.IsAssigned);
        if (slot == null)
        {
            Debug.LogWarning("[AbilityUIC] No free ability slot available.");
            return;
        }

        slot.Assign(config);
        m_assigned[config] = slot;
    }

    public void RemoveDisplay(AbilityConfig config)
    {
        if (config == null || !m_assigned.TryGetValue(config, out AbilityDisplay slot)) return;
        m_assigned.Remove(config);
        slot.Unassign();
    }

    public void OnAbilityUsed(AbilityConfig config)
    {
        if (m_assigned.TryGetValue(config, out AbilityDisplay slot))
            slot.PlayUsedFeedback();
    }

    public void UpdateCooldown(AbilityConfig config, float remaining, float total)
    {
        if (m_assigned.TryGetValue(config, out AbilityDisplay slot))
            slot.UpdateCooldown(remaining, total);
    }
}
