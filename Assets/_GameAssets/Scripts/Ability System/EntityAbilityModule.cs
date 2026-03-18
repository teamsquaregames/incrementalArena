using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using Stats;
using UnityEngine;

public class EntityAbilityModule : EntityModule
{
    public const string AUTO_ATTACK_CLIP_SLOT = "AutoAttack";
    public const string ABILITY_CLIP_SLOT     = "Ability";
    public const string ANIMATOR_BOOL         = "IsAttacking";
    public const string ANIMATOR_TRIGGER      = "Ability";

    [Header("Auto Attack")]
    [SerializeField] private AbilityConfig m_autoAttack;

    [Header("Abilities")]
    [SerializeField] private List<AbilityConfig> m_abilities = new List<AbilityConfig>();
    [SerializeField] private bool m_stopMovementOnCast = true;

    [SerializeField] private Animator m_animator;

    private AnimatorOverrideController   m_overrideController;

    private AbilityConfig             m_activeAbility;
    private AbilityContext            m_activeContext;
    private bool                      m_isAutoAttack;
    private int                       m_comboIndex;
    private Dictionary<string, float> m_cooldowns = new();

    public AbilityConfig       AutoAttack => m_autoAttack;
    public List<AbilityConfig> Abilities  => m_abilities;

    /// <summary>True only while a non-auto ability animation is running.</summary>
    public bool IsUsingAbility => m_activeAbility != null && !m_isAutoAttack;

    /// <summary>True while any ability (including auto-attack) animation is running.</summary>
    public bool IsBusy => m_activeAbility != null;

    /// <summary>The config of the ability currently being cast, or null if none.</summary>
    public AbilityConfig ActiveAbility => m_activeAbility;

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

    private void SetAbilityClip(AnimationClip clip, bool isAutoAttack)
    {
        if (m_overrideController == null || clip == null) return;

        if (isAutoAttack)
        {
            m_overrideController[AUTO_ATTACK_CLIP_SLOT] = clip;

            float animatorSpeed = 1f;
            if (Owner.TryGetModule(out EntityStatModule statModule))
            {
                float attackSpeed = statModule.GetValue(StatType.AttackSpeed);
                if (attackSpeed > 0f)
                    animatorSpeed = attackSpeed * clip.length;
            }

            m_animator.speed = animatorSpeed;
        }
        else
        {
            m_overrideController[ABILITY_CLIP_SLOT] = clip;
            m_animator.speed = 1f;
        }
    }

    // -------------------------------------------------------------------------
    // Auto Attack — no cooldown, interruptible by abilities
    // -------------------------------------------------------------------------
    public bool TryUseAutoAttack(Vector3 targetPos)
    {
        if (m_autoAttack == null) return false;

        // Don't re-trigger while already auto-attacking
        if (IsBusy) return false;

        m_comboIndex = m_comboIndex % m_autoAttack.steps.Count;
        return StartAbility(m_autoAttack, targetPos, isAutoAttack: true);
    }

    // -------------------------------------------------------------------------
    // Regular abilities — subject to cooldown, interrupt auto-attacks
    // -------------------------------------------------------------------------
    public bool TryUseAbility(AbilityConfig ability, Vector3 targetPos)
    {
        if (!CanUse(ability)) return false;

        m_cooldowns[ability.abilityName] = ability.cooldown;
        return StartAbility(ability, targetPos, isAutoAttack: false);
    }

    // -------------------------------------------------------------------------
    // Shared activation logic
    // -------------------------------------------------------------------------
    private bool StartAbility(AbilityConfig ability, Vector3 targetPos, bool isAutoAttack)
    {
        Vector3 direction = (targetPos - Owner.transform.position).SetY(0);
        if (direction.sqrMagnitude > 0.001f)
            Owner.transform.rotation = Quaternion.LookRotation(direction);

        int stepIndex = isAutoAttack ? m_comboIndex : 0;

        m_activeAbility = ability;
        m_isAutoAttack  = isAutoAttack;
        m_activeContext = new AbilityContext
        {
            Caster         = Owner,
            TargetPosition = targetPos,
            AbilityConfig  = ability
        };

        SetAbilityClip(ability.steps[stepIndex].abilityClip, isAutoAttack);

        if (!isAutoAttack && m_stopMovementOnCast)
        {
            if (Owner.TryGetModule(out EntityMovementModule movementModule))
                movementModule.SetMoveInput(Vector2.zero);
        }

        if (isAutoAttack)
            m_animator.SetBool(ANIMATOR_BOOL, true);
        else
            m_animator.SetTrigger(ANIMATOR_TRIGGER);

        return true;
    }

    // -------------------------------------------------------------------------
    // Animation callbacks (called by animation events on the clip)
    // -------------------------------------------------------------------------
    internal void HandleAnimationStart()
    {
    }

    internal void HandleAnimationEnd()
    {
        m_animator.SetBool(ANIMATOR_BOOL, false);
        m_animator.speed = 1f;

        if (m_isAutoAttack)
            m_comboIndex++;

        m_activeAbility = null;
        m_activeContext = null;
        m_isAutoAttack  = false;
    }

    internal void HandleAnimationEvent()
    {
        if (m_activeAbility == null) return;

        // Use the current combo index for auto-attacks (not yet incremented — that happens in HandleAnimationEnd)
        int step = m_isAutoAttack ? m_comboIndex : 0;
        AbilityStep activeStep = m_activeAbility.steps[step];

        LeanPool.Spawn(
            activeStep.mainVfx,
            activeStep.mainVFXPosition == VFXPosition.Target
                ? m_activeContext.TargetPosition
                : transform.position.OffsetY(0.75f),
            transform.rotation
        );

        List<Entity> targets = ResolveTargets(m_activeContext.TargetPosition, m_activeAbility.aoeRadius);
        foreach (var target in targets)
        {
            LeanPool.Spawn(activeStep.hitVfx, target.transform.position.OffsetY(0.5f), Quaternion.identity);
            foreach (var entry in activeStep.effects)
            {
                m_activeContext.Value = entry.value;
                entry.effect?.Execute(m_activeContext, target);
            }
        }
    }

    private List<Entity> ResolveTargets(Vector3 position, float radius)
    {
        var results        = new List<Entity>();
        var hits           = Physics.OverlapSphere(position, radius);
        var casterCollider = m_activeContext.Caster.GetComponent<Collider>();

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
        m_animator.speed   = 1f;
        m_activeAbility    = null;
        m_activeContext    = null;
        m_isAutoAttack     = false;
        ResetCombo();
    }

    public void ResetCombo()
    {
        m_comboIndex = 0;
    }

    private bool CanUse(AbilityConfig ability)
    {
        if (m_cooldowns.TryGetValue(ability.abilityName, out float cd) && cd > 0f) return false;
        return true;
    }

    public float GetCooldownRemaining(AbilityConfig ability)
    {
        return m_cooldowns.TryGetValue(ability.abilityName, out float cd) ? cd : 0f;
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