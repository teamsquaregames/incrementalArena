using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using UnityEngine;

public class EntityAbilityModule : EntityModule
{
    public const string ABILITY_CLIP_SLOT = "AbilityClip";
    public const string ANIMATOR_BOOL     = "IsAttacking";

    [Header("Auto Attack")]
    [SerializeField] private AbilitySO m_autoAttack;
    [SerializeField] private Animator  m_animator;

    private AnimatorOverrideController m_overrideController;

    private AbilitySO    m_activeAbility;
    private AbilityContext m_activeContext;
    private Dictionary<string, float> m_cooldowns = new();

    public AbilitySO AutoAttack => m_autoAttack;

    #region Module

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_animator = GetComponentInChildren<Animator>();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        InitOverrideController();
    }

    #endregion
    
    private void InitOverrideController()
    {
        if (m_animator == null) return;
        
        m_overrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);
        m_animator.runtimeAnimatorController = m_overrideController;
    }
    
    private void SetAbilityClip(AnimationClip clip)
    {
        if (m_overrideController == null || clip == null) return;
        m_overrideController[ABILITY_CLIP_SLOT] = clip;
    }

    
    public bool TryUseAbility(AbilitySO ability, Vector3 targetPos)
    {
        if (!CanUse(ability))
        {
            m_animator.SetBool(ANIMATOR_BOOL, false);
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

        // Swap the clip BEFORE setting the bool so the state machine picks it up
        // immediately when it transitions on the same frame.
        SetAbilityClip(ability.abilityClip);
        m_animator.SetBool(ANIMATOR_BOOL, true);
        return true;
    }
    
    internal void HandleAnimationStart()
    {
    }
    
    internal void HandleAnimationEnd()
    {
        m_animator.SetBool(ANIMATOR_BOOL, false);
        m_activeAbility = null;
        m_activeContext = null;
    }
    
    internal void HandleAnimationEvent()
    {
        if (m_activeAbility == null) return;
        
        LeanPool.Spawn(
            m_activeAbility.mainVfx,
            m_activeAbility.mainVFXPosition == VFXPosition.Target
                ? m_activeContext.TargetPosition
                : transform.position.OffsetY(0.75f),
            transform.rotation
        );

        List<Entity> targets = ResolveTargets(m_activeContext.TargetPosition, m_activeAbility.aoeRadius);
        foreach (var target in targets)
        {
            LeanPool.Spawn(m_activeAbility.hitVfx, target.transform.position.OffsetY(0.5f), Quaternion.identity);
            foreach (var entry in m_activeAbility.effects)
            {
                m_activeContext.Value = entry.value;
                entry.effect?.Execute(m_activeContext, target);
            }
        }
    }

    private List<Entity> ResolveTargets(Vector3 position, float radius)
    {
        var results         = new List<Entity>();
        var hits            = Physics.OverlapSphere(position, radius);
        var casterCollider  = m_activeContext.Caster.GetComponent<Collider>();

        foreach (var hit in hits)
        {
            if (hit == casterCollider) continue;

            if (EntityManager.Instance != null &&
                EntityManager.Instance.EntitiesByCollider.TryGetValue(hit, out Entity entity))
            {
                results.Add(entity);
            }
        }

        return results;
    }

    public void CancelAbility()
    {
        m_animator.SetBool(ANIMATOR_BOOL, false);
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