using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using UnityEngine.XR;
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

    [SerializeField] private Animator m_animator;

    private AnimatorOverrideController m_overrideController;

    [InlineProperty] private AbilityConfig m_activeAbility;
    private AbilityContext m_activeContext;
    private bool m_isAutoAttack;
    private int m_stepIndex;
    private Dictionary<string, float> m_cooldowns = new();

    public AbilityConfig AutoAttack => m_autoAttack;
    public List<AbilityConfig> Abilities => m_abilities;

    public bool IsUsingAbility => m_activeAbility != null && !m_isAutoAttack;
    public bool IsBusy => m_activeAbility != null;
    public bool IsCastRooted;
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

        m_stepIndex = m_stepIndex % m_autoAttack.steps.Count;
        return StartAbility(m_autoAttack, aimPosition, isAutoAttack: true);
    }


    public bool TryUseAbility(AbilityConfig ability, Vector3 aimPosition)
    {
        if (!CanUse(ability)) return false;

        m_cooldowns[ability.abilityName] = ability.cooldown;
        m_stepIndex = 0;
        return StartAbility(ability, aimPosition, isAutoAttack: false);
    }


    private bool StartAbility(AbilityConfig ability, Vector3 aimPosition, bool isAutoAttack)
    {
        // this.Log($"Starting {(isAutoAttack ? $"auto-attack {m_stepIndex}" : ability.abilityName)} toward {aimPosition}");

        Vector3 direction = (aimPosition - Owner.transform.position).SetY(0);
        if (direction.sqrMagnitude > 0.001f)
            Owner.transform.rotation = Quaternion.LookRotation(direction);

        m_activeAbility = ability;
        m_isAutoAttack = isAutoAttack;
        m_activeContext = new AbilityContext
        {
            Caster = Owner,
            AimPosition = aimPosition,
            ClosestEntity = CursorBrainModule.GetClosestEnemyInCursor(Owner),
            AbilityConfig = ability
        };

        SetAbilityClip(ability.steps[m_stepIndex].abilityClip, isAutoAttack);

        if (ability.steps[m_stepIndex].isRooting)
        {
            if (Owner.TryGetModule(out EntityMovementModule movementModule))
                movementModule.SetMoveInput(Vector2.zero);
        }

        if (isAutoAttack)
        {
            m_animator.SetBool(IS_ATTACKING, true);
            m_animator.CrossFadeInFixedTime(AUTO_ATTACK_CLIP_SLOT, m_stepIndex == 0 ? 0.1f : 0f);
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
        // this.Log($"Handling animation active event. stepIndex={m_stepIndex}, activeAbility={m_activeAbility?.abilityName}, isAutoAttack={m_isAutoAttack}");
        if (m_activeAbility == null) return;

        // Use the current step index for auto-attacks (not yet incremented — that happens in HandleAnimationEnd)
        AbilityStep activeStep = m_activeAbility.steps[m_stepIndex];


        List<Entity> targets = ResolveApplication.ResolveApplications(activeStep.targetingInfo, m_activeContext);
        foreach (var item in targets)
        {
            // this.Log($"Resolved targets: {item.name}, position: {item.transform.position}");
        }

        HandleEffects(activeStep, targets);
        HandleVFXs(activeStep, targets);

        
        if (activeStep.isRooting)
        {
            if (Owner.TryGetModule(out EntityMovementModule movementModule))
                movementModule.SetMoveInput(Vector2.zero);
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

        m_stepIndex++;
        m_activeContext.CurrentStepIndex = m_stepIndex;
    }


    internal void HandleAnimationEnd()
    {
        // this.Log("Handling animation end event");
        m_animator.SetBool(IS_ATTACKING, false);
        m_animator.speed = 1f;

        if (!m_isAutoAttack)
            m_stepIndex = 0;

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
        // this.Log("Cancelling everything");

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
        m_stepIndex = 0;
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


    private void HandleVFXs(AbilityStep activeStep, List<Entity> targets = null)
    {
        /// Ability VFX
        if (activeStep.mainVfx != null)
            LeanPool.Spawn(
                activeStep.mainVfx,
                activeStep.mainVFXPosition == VFXPosition.Target
                    ? m_activeContext.AimPosition
                    : transform.position.OffsetY(0f),
                transform.rotation
            );
        if (activeStep.mainVfxGraph != null)
        {
            LeanPool.Spawn(
                activeStep.mainVfxGraph,
                activeStep.mainVFXPosition == VFXPosition.Target
                    ? m_activeContext.AimPosition
                    : transform.position.OffsetY(0f),
                transform.rotation
            );
        }

        /// Hits
        if (activeStep.hitVfx != null)
        {
            foreach (var target in targets)
            {
                LeanPool.Spawn(activeStep.hitVfx, target.transform.position.OffsetY(1), Quaternion.identity, target.transform);
            }
        }
        if (activeStep.hitVfxGraph != null)
        {
            foreach (var target in targets)
            {
                LeanPool.Spawn(activeStep.hitVfxGraph, target.transform.position.OffsetY(1), Quaternion.identity, target.transform);
            }
        }
    }

    private void HandleEffects(AbilityStep activeStep, List<Entity> targets)
    {
        foreach (var target in targets)
        {
            foreach (var entry in activeStep.effects)
            {
                m_activeContext.Value = entry.value;
                entry.effect?.Execute(m_activeContext, target);
            }
        }
    }
}