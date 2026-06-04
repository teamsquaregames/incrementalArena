using System;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using Stats;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;
using System.Collections;

public class EntityHealthModule : EntityModule
{
    public Action<double, double, bool> OnHealthChanged;
    public Action<double, bool> OnDamageTaken;
    public Action<double, double> OnHealed;
    public Action OnDeath;
    public Action OnDeathAnimEnd;

    [Title("Dependencies")]
    [SerializeField] private GameObject m_deathFxPefab;

    [FoldoutGroup("Feedback settings"), SerializeField] private Vector3 punchScale = new Vector3(0.3f, -0.2f, 0f);
    [FoldoutGroup("Feedback settings"), SerializeField, Min(0f)] private float punchDuration = 0.35f;
    [FoldoutGroup("Feedback settings"), SerializeField, Min(1)] private int punchVibrato = 6;
    [FoldoutGroup("Feedback settings"), SerializeField, Range(0f, 1f)] private float punchElasticity = 0.5f;

    [Title("Death settings")]
    [SerializeField, Min(0f)] private float m_deathDespawnDelay = 5f;

    protected double m_currentHealth;
    protected bool m_isDead;
    private Tween m_punchTween;
    protected EntityStatModule m_statModule;
    protected EntityAbilityModule m_abilityModule;

    public bool IsDead => m_isDead;
    public double MaxHealth
    {
        get
        {
            if (Owner.TryGetModule(out EntityStatModule statModule))
            {
                // this.Log($"{Owner} MaxHealth value retrieved from StatModule: {statModule.GetValue(StatType.MaxHealth)}");
                return statModule.GetValue(StatType.MaxHealth);
            }
            else
            {
                this.LogWarning("No StatModule attached. Couldn't get MaxHealth value. Returning 100 as default");
                return 100;

            }
        }
    }

    public override void CacheReferences()
    {
        base.CacheReferences();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        m_isDead = false;
    }

    public override void OnAllModuleInitialized()
    {
        base.OnAllModuleInitialized();
        Owner.TryGetModule(out m_statModule);
        if (Owner.TryGetModule(out m_abilityModule))
            m_abilityModule.OnDamageDealt += LifestealHeal;
        m_currentHealth = MaxHealth;
        // this.Log($"Initializing EntityHealthModule for {Owner}. MaxHealth: {MaxHealth}, CurrentHealth: {m_currentHealth}");
    }

    protected virtual void PlayDamageFeedback(float damagePercentage)
    {
        PlayPunchScale();

        if (Owner.TryGetModule(out EntitySheenModule sheenModule))
        {
            sheenModule.PlayWhiteSheen();
        }

        SoundManager.Instance.PlaySound(SoundKeys.SFX_Impact);
    }

    protected virtual void PlayBlockFeedback()
    {
        SoundManager.Instance.PlaySound(SoundKeys.sfx_block);
    }

    private void PlayPunchScale()
    {
        m_punchTween?.Kill(complete: true);
        Owner.transform.localScale = Vector3.one;

        m_punchTween = Owner.transform
            .DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity)
            .SetUpdate(UpdateType.Normal)
            .SetLink(Owner.gameObject);
    }

    public void UpdateCurrentHealth()
    {
        m_currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, true);
    }

    [Button]
    public double TakeDamage(double amount, bool isCrit, bool noArmor = false, bool suppressFeedback = false)
    {
        if (m_isDead) return 0d;

        if (!noArmor)
            amount -= m_statModule.GetValue(StatType.Armor);

        if (amount <= 0f)
        {
            OnDamageTaken?.Invoke(0, false);
            PlayBlockFeedback();
            return 0d;
        }

        double previous = m_currentHealth;
        m_currentHealth = Math.Max(0d, m_currentHealth - amount);
        double delta = m_currentHealth - previous;

        if (!suppressFeedback)
        {            
            PlayDamageFeedback((float)(amount / MaxHealth));
            OnDamageTaken?.Invoke(amount, isCrit);
        }

        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, suppressFeedback);

        if (m_currentHealth <= 0f || GameConfig.Instance.cheatSettings.oneHitKill)
        {
            //Todo : remettre cette ligne quand les anims serotn branchées
            //StartDeathAnimation();

            Die();
        }
        return amount;
    }

    #region Heal
    protected virtual void Heal(double amount, bool suppressFeedback = false)
    {
        if (m_isDead || amount <= 0f) return;

        double previous = m_currentHealth;
        m_currentHealth = Math.Min(MaxHealth, m_currentHealth + amount);
        double delta = m_currentHealth - previous;

        if (!suppressFeedback)
        {
            // this.Log("Playing heal feedback");
            // SoundManager.Instance.PlaySound(SoundKeys.SFX_Heal);
        }

        OnHealed?.Invoke(amount, m_currentHealth);
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, suppressFeedback);
    }

    public void RoundHeal()
    {
        Heal(m_statModule.GetValue(StatType.RoundRegen) / 100f * MaxHealth);
    }

    private void LifestealHeal(double damageDealt)
    {
        if (m_statModule.GetValue(StatType.Lifesteal) <= 0f) return;
        // this.Log($"LifestealHeal triggered. Damage dealt: {damageDealt}, Lifesteal%: {m_statModule.GetValue(StatType.Lifesteal)}");
        Heal(damageDealt * m_statModule.GetValue(StatType.Lifesteal) / 100f, suppressFeedback: true);
    }
    #endregion

    public override void Cleanup()
    {
        if (m_abilityModule != null)
            m_abilityModule.OnDamageDealt -= LifestealHeal;
        OnHealthChanged = null;
        OnDamageTaken = null;
        OnHealed = null;
        OnDeath = null;
        OnDeathAnimEnd = null;


    }

    public virtual void Die()
    {
        // this.Log($"{Owner} has died.");
        if (GameConfig.Instance.cheatSettings.npcImmortality)
            return;
        m_isDead = true;

        LeanPool.Spawn(m_deathFxPefab, transform.position + Vector3.up * Owner.Height / 2f, Quaternion.Euler(0, Random.Range(0, 360), 0));

        if (Owner.TryGetModule(out EntityTeamModule teamModule) && teamModule.Team == Team.Enemy)
            GameData.Instance.IncrementTrackedValue(TrackedValueType.EnemiesKilledThisRun);

        OnDeath?.Invoke();
        StartCoroutine(DeathAnimCR());
    }

    private IEnumerator DeathAnimCR()
    {
        SoundManager.Instance.PlaySound(SoundKeys.SFX_Groan);
        m_punchTween?.Kill(complete: true);
        Owner.Animator.Play("Death");
        Owner.CustomRE.ChangeFloat("_Saturation", 0f);

        yield return new WaitForSeconds(m_deathDespawnDelay);

        OnDeathAnimEnd?.Invoke();
    }
}