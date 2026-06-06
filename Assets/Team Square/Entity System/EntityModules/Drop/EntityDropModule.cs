using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using Utils;

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
        foreach (RewardEntry entry in m_dropEntries)
        {
            // this.Log($"Adding drop reward entry for entity {Owner.name} with stat multiplier {StatManager.Instance.GetDefinitionValue(Owner.EntityType, StatType.DropRewardMultiplier)}: {entry.rewardObject} {entry.value}");

            RewardEntry entryCopy = new RewardEntry(entry.rewardObject, entry.value);
            entryCopy.value *= StatManager.Instance.GetDefinitionValue(Owner.EntityType, StatType.DropRewardMultiplier);
            LevelManager.Instance.CrowdRewards.AddRewardEntry(entryCopy);
        }
    }
}
