using UnityEngine;

public class EntityAbilityUIModule : EntityModule
{
    private AbilityUIC m_abilityUIC => UIManager.Instance.GetCanvas<GameCanvas>().GetContainer<AbilityUIC>();
    private EntityAbilityModule m_abilityModule;

    public override void OnAllModuleInitialized()
    {
        if (!Owner.TryGetModule(out m_abilityModule))
        {
            Debug.LogWarning($"[EntityAbilityUIModule] No EntityAbilityModule found on {Owner.name}.");
            return;
        }

        m_abilityModule.OnAbilityUsed   += HandleAbilityUsed;
        m_abilityModule.OnAbilityAdded  += HandleAbilityAdded;
        m_abilityModule.OnAbilityRemoved += HandleAbilityRemoved;
        
        if (m_abilityModule.AutoAttack != null)
            m_abilityUIC.AddDisplay(m_abilityModule.AutoAttack);

        foreach (AbilityConfig ability in m_abilityModule.Abilities)
            m_abilityUIC.AddDisplay(ability);
    }

    private void OnDestroy()
    {
        if (m_abilityModule == null) return;

        m_abilityModule.OnAbilityUsed    -= HandleAbilityUsed;
        m_abilityModule.OnAbilityAdded   -= HandleAbilityAdded;
        m_abilityModule.OnAbilityRemoved -= HandleAbilityRemoved;

        if (m_abilityModule.AutoAttack != null)
            m_abilityUIC.RemoveDisplay(m_abilityModule.AutoAttack);

        foreach (AbilityConfig ability in m_abilityModule.Abilities)
            m_abilityUIC.RemoveDisplay(ability);
    }

    private void Update()
    {
        if (m_abilityModule == null) return;

        // ── Regular abilities ─────────────────────────────────────────────────
        foreach (AbilityConfig ability in m_abilityModule.Abilities)
        {
            float remaining = m_abilityModule.GetCooldownRemaining(ability);
            m_abilityUIC.UpdateCooldown(ability, remaining, ability.Cooldown());
        }
    }

    private void HandleAbilityUsed(AbilityConfig ability)
    {
        m_abilityUIC.OnAbilityUsed(ability);

        if (ability != m_abilityModule.AutoAttack) return;
    }

    private void HandleAbilityAdded(AbilityConfig ability)   => m_abilityUIC.AddDisplay(ability);
    private void HandleAbilityRemoved(AbilityConfig ability)  => m_abilityUIC.RemoveDisplay(ability);
}
