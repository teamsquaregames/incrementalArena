using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using Utils;

public class EntityAbilityModule : EntityModule
{
    public const string AUTO_ATTACK_CLIP_SLOT = "AutoAttack";
    public const string ABILITY_CLIP_SLOT = "Ability";
    public const string IS_ATTACKING = "IsAttacking";
    public const string TRIGGER_ABILITY = "Ability";

    [TitleGroup("References")]
    [SerializeField] private AbilityApplicationGizmos m_gizmos;

    [Header("Auto Attack")]
    [SerializeField, InlineEditor] private AbilityConfig m_autoAttack;

    [Header("Abilities")]
    [SerializeField] private List<AbilityConfig> m_abilities = new List<AbilityConfig>();
    [SerializeField] private bool m_stopMovementOnCast = true;

    [SerializeField] private Animator m_animator;

    private AnimatorOverrideController m_overrideController;

    [InlineProperty] private AbilityConfig m_activeAbility;
    private AbilityContext m_activeContext;
    private bool m_isAutoAttack;
    private int m_comboIndex;
    private Dictionary<string, float> m_cooldowns = new();

    public AbilityConfig AutoAttack => m_autoAttack;
    public List<AbilityConfig> Abilities => m_abilities;

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


    public bool TryUseAutoAttack(Vector3 aimPosition)
    {
        if (m_autoAttack == null) return false;

        // Don't re-trigger while already auto-attacking
        if (IsBusy) return false;

        m_comboIndex = m_comboIndex % m_autoAttack.steps.Count;
        return StartAbility(m_autoAttack, aimPosition, isAutoAttack: true);
    }


    public bool TryUseAbility(AbilityConfig ability, Vector3 aimPosition)
    {
        if (!CanUse(ability)) return false;

        m_cooldowns[ability.abilityName] = ability.cooldown;
        return StartAbility(ability, aimPosition, isAutoAttack: false);
    }


    private bool StartAbility(AbilityConfig ability, Vector3 aimPosition, bool isAutoAttack)
    {
        //this.Log($"Starting {(isAutoAttack ? $"auto-attack {m_comboIndex}" : ability.abilityName)} toward {aimPosition}");
        Vector3 direction = (aimPosition - Owner.transform.position).SetY(0);
        if (direction.sqrMagnitude > 0.001f)
            Owner.transform.rotation = Quaternion.LookRotation(direction);

        int stepIndex = isAutoAttack ? m_comboIndex : 0;

        m_activeAbility = ability;
        m_isAutoAttack = isAutoAttack;
        m_activeContext = new AbilityContext
        {
            Caster = Owner,
            AimPosition = aimPosition,
            ClosestEntity = CursorBrainModule.GetClosestEnemyInCursor(Owner),
            AbilityConfig = ability
        };

        SetAbilityClip(ability.steps[stepIndex].abilityClip, isAutoAttack);

        if (!isAutoAttack && m_stopMovementOnCast)
        {
            if (Owner.TryGetModule(out EntityMovementModule movementModule))
                movementModule.SetMoveInput(Vector2.zero);
        }

        if (isAutoAttack)
        {
            m_animator.SetBool(IS_ATTACKING, true);
            m_animator.CrossFadeInFixedTime(AUTO_ATTACK_CLIP_SLOT, m_comboIndex == 0 ? 0.1f : 0f);
        }
        else
            m_animator.SetTrigger(TRIGGER_ABILITY);

        return true;
    }


    internal void HandleAnimationStart()
    {
    }


    internal void HandleAnimationActive()
    {
        //this.Log("Handling animation active event");
        if (m_activeAbility == null) return;

        // Use the current combo index for auto-attacks (not yet incremented — that happens in HandleAnimationEnd)
        int step = m_isAutoAttack ? m_comboIndex : 0;
        AbilityStep activeStep = m_activeAbility.steps[step];

        LeanPool.Spawn(
            activeStep.mainVfx,
            activeStep.mainVFXPosition == VFXPosition.Target
                ? m_activeContext.AimPosition
                : transform.position.OffsetY(0.75f),
            transform.rotation
        );

        List<Entity> targets = ResolveApplication.ResolveApplications(activeStep.targetingInfo, m_activeContext);
        foreach (var target in targets)
        {
            LeanPool.Spawn(activeStep.hitVfx, target.transform.position.OffsetY(0.5f), Quaternion.identity);
            foreach (var entry in activeStep.effects)
            {
                m_activeContext.Value = entry.value;
                entry.effect?.Execute(m_activeContext, target);
            }
        }

        if (m_gizmos != null)
        {
            m_gizmos.applicationInfo = activeStep.applicationInfos.Count > 0 ? activeStep.applicationInfos[0] : null;
            m_gizmos.targetingInfo = activeStep.targetingInfo;
            m_gizmos.position = activeStep.targetingInfo.quickTarget switch
            {
                TargetingInfo.QuickTarget.Self => m_activeContext.Caster.transform.position,
                TargetingInfo.QuickTarget.Current => m_activeContext.ClosestEntity != null ? m_activeContext.ClosestEntity.transform.position : m_activeContext.AimPosition,
                // TargetingInfo.QuickTarget.Cursor => UtilsClass.GetMouseWorldPosition(),
                _ => m_activeContext.AimPosition
            };
        }
    }
    
    internal void HandleAnimationEnd()
    {
        // this.Log("Handling animation end event");
        m_animator.SetBool(IS_ATTACKING, false);
        m_animator.speed = 1f;

        if (m_isAutoAttack)
            m_comboIndex++;

        m_activeAbility = null;
        m_activeContext = null;
        m_isAutoAttack = false;
    }

    internal void HandleAnimationInterrupt()
    {
        // this.Log("Handling animation interrupt event");
        m_animator.SetBool(IS_ATTACKING, false);
        m_animator.speed = 1f;

        m_activeAbility = null;
        m_activeContext = null;
        m_isAutoAttack = false;
    }


    public void CancelEverything()
    {
        //this.Log("Cancelling everything");

        m_activeAbility = null;
        m_animator.speed = 1f;
        m_isAutoAttack = false;
        m_activeContext = null;

        m_animator.SetBool(IS_ATTACKING, false);
        ResetCombo();
    }

    public void ResetCombo()
    {
        // this.Log("Resetting combo");
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