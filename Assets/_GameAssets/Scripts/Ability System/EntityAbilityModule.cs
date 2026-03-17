using System.Collections.Generic;
using UnityEngine;

public class EntityAbilityModule : EntityModule
{
    [Header("Auto Attack")]
    [SerializeField] private AbilitySO m_autoAttack;
    [SerializeField] private Animator m_animator;

    private AbilitySO m_activeAbility;
    private AbilityContext m_activeContext;
    private Dictionary<string, float> m_cooldowns = new();

    public override void CacheReferences()
    {
        base.CacheReferences();
        m_animator = GetComponentInChildren<Animator>();
    }

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

    public bool CanUse(AbilitySO ability)
    {
        //if (m_phase != AbilityPhase.None) return false;
        if (m_cooldowns.TryGetValue(ability.abilityName, out float cd) && cd > 0f) return false;
        return true;
    }

    public float GetCooldownRatio(AbilitySO ability)
    {
        if (!m_cooldowns.TryGetValue(ability.abilityName, out float cd)) return 0f;
        return Mathf.Clamp01(cd / ability.cooldown);
    }

    internal void HandleAnimationEvent()
    {
        print("HandleAnimationEvent");
        if (m_activeAbility == null) return;

        foreach (var entry in m_activeAbility.effects)
        {
            print(entry);
            m_activeContext.Value = entry.value;
            entry.effect?.Execute(m_activeContext);
        }
    }

    internal void HandleAnimationEnd()
    {
        m_activeAbility = null;
        m_activeContext = null;
    }
    
    internal void HandleAnimationStart()
    {
        //todo : spawn vfx
    }

    private void Update()
    {
        UpdateAutoAttack();
        UpdateCooldowns();
    }

    private void UpdateAutoAttack()
    {
        if (HasEnemyInRange())
        {
            TryUseAbility(m_autoAttack,
                CursorManager.Instance.EntitiesInCursor[Random.Range(0, CursorManager.Instance.EntitiesInCursor.Count)]
                    .transform.position);
        }
    }

    private bool HasEnemyInRange()
    {
        bool isPlayer = Owner.TryGetModule(out EntityTeamModule teamModule) && teamModule.Team == Team.Player;

        if (isPlayer)
        {
            foreach (var entity in CursorManager.Instance.EntitiesInCursor)
            {
                if (entity.TryGetModule(out EntityTeamModule tm) && tm.Team == Team.Enemy)
                    return true;
            }
            return false;
        }
        else
        {
            var player = EntityManager.Instance.Player;
            if (player == null) return false;
            return Vector3.Distance(Owner.transform.position, player.transform.position) <= m_autoAttack.range;
        }
    }

    private void UpdateCooldowns()
    {
        var keys = new List<string>(m_cooldowns.Keys);
        foreach (var k in keys)
            m_cooldowns[k] = Mathf.Max(0f, m_cooldowns[k] - Time.deltaTime);
    }
}
