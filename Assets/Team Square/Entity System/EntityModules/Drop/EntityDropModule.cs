using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityDropModule : EntityModule
{
    [TitleGroup("Drop Settings")]
    [SerializeField] private List<RewardEntry> m_dropEntries = new List<RewardEntry>();

    private EntityHealthModule m_healthModule;

    public override void OnAllModuleInitialized()
    {
        base.OnAllModuleInitialized();

        if (Owner.TryGetModule(out m_healthModule))
        {
            m_healthModule.OnDeath += OnDeath;
        }
    }

    private void OnDeath()
    {
        m_healthModule.OnDeath -= OnDeath;
        foreach (var entry in m_dropEntries)
        {
            LevelManager.Instance.CrowdRewards.AddRewardEntry(entry);
        }
    }
}
