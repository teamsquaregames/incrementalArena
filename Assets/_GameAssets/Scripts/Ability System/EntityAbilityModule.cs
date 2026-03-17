using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using UnityEngine;

public class EntityAbilityModule : EntityModule
{
    [Header("Auto Attack")]
    [SerializeField] private AbilitySO m_autoAttack;
    [SerializeField] private Animator m_animator;

    private AbilitySO m_activeAbility;
    private AbilityContext m_activeContext;
    private Dictionary<string, float> m_cooldowns = new();

    public AbilitySO AutoAttack => m_autoAttack;

    #region Module

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_animator = GetComponentInChildren<Animator>();
    }

    #endregion

    
    public bool TryUseAbility(AbilitySO ability, Vector3 targetPos)
    {
        if (!CanUse(ability))
        {
            m_animator.SetBool(ability.animatorBoolName, false);
            return false;
        }

        m_activeAbility = ability;
        m_activeContext = new AbilityContext
        {
            Caster         = gameObject,
            TargetPosition = targetPos,
            AbilityData    = ability
        };

        m_cooldowns[ability.abilityName] = ability.cooldown;
        m_animator.SetBool(ability.animatorBoolName, true);
        return true;
    }
    
    internal void HandleAnimationStart()
    {
    }
    
    internal void HandleAnimationEnd()
    {
        m_activeAbility = null;
        m_activeContext = null;
    }
    
    internal void HandleAnimationEvent()
    {
        if (m_activeAbility == null) return;
        
        LeanPool.Spawn(m_activeAbility.vfx, m_activeContext.TargetPosition, Quaternion.identity);

        foreach (var entry in m_activeAbility.effects)
        {
            print(entry);
            m_activeContext.Value = entry.value;
            entry.effect?.Execute(m_activeContext);
        }
    }

    public void CancelAbility()
    {
        m_animator.SetBool("IsAttacking", false); 
        m_activeAbility = null;
        m_activeContext = null;
    }
    
    
    

    private bool CanUse(AbilitySO ability)
    {
        if (m_cooldowns.TryGetValue(ability.abilityName, out float cd) && cd > 0f) return false;
        return true;
    }
    

    private void Update()
    {
        UpdateCooldowns();
    }

    private void UpdateCooldowns()
    {
        var keys = new List<string>(m_cooldowns.Keys);
        foreach (var k in keys)
            m_cooldowns[k] = Mathf.Max(0f, m_cooldowns[k] - Time.deltaTime);
    }
}
