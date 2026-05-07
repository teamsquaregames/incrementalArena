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
    public Action<float, float, float, bool, bool> OnHealthChanged;
    public Action<float, float> OnDamageTaken;
    public Action<float, float> OnHealed;
    public Action OnDeathStart;
    public Action OnDeath;

    [Title("Dependencies")]
    [SerializeField, Required] private CustomRE m_customRE;
    [SerializeField] private GameObject m_deathFxPefab;
    [SerializeField] private RagdollManager m_ragdollManager;

    [FoldoutGroup("Feedback settings"), SerializeField] private Vector3 punchScale = new Vector3(0.3f, -0.2f, 0f);
    [FoldoutGroup("Feedback settings"), SerializeField, Min(0f)] private float punchDuration = 0.35f;
    [FoldoutGroup("Feedback settings"), SerializeField, Min(1)] private int punchVibrato = 6;
    [FoldoutGroup("Feedback settings"), SerializeField, Range(0f, 1f)] private float punchElasticity = 0.5f;

    [Title("Death settings")]
    [SerializeField, Min(0f)] private float m_deathDespawnDelay = 5f;

    private float m_currentHealth;
    protected bool m_isDead;
    private Tween m_punchTween;
    protected EntityStatModule m_statModule;

    public bool IsDead => m_isDead;
    public float MaxHealth
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
        m_currentHealth = MaxHealth;
        // this.Log($"Initializing EntityHealthModule for {Owner}. MaxHealth: {MaxHealth}, CurrentHealth: {m_currentHealth}");
    }

    protected virtual void PlayDamageFeedback()
    {
        PlayPunchScale();

        if (Owner.TryGetModule(out EntitySheenModule sheenModule))
        {
            sheenModule.PlayWhiteSheen();
        }

        SoundManager.Instance.PlaySound(SoundKeys.SFX_Impact);
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
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, 0f, false, true);
    }

    [Button]
    public void TakeDamage(float amount, bool isCrit, bool suppressFeedback = false)
    {
        if (m_isDead || amount <= 0f) return;

        float previous = m_currentHealth;
        m_currentHealth = Mathf.Max(0f, m_currentHealth - amount);
        float delta = m_currentHealth - previous;

        if (!suppressFeedback)
            PlayDamageFeedback();

        OnDamageTaken?.Invoke(amount, m_currentHealth);
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, delta, isCrit, suppressFeedback);

        if (m_currentHealth <= 0f || GameConfig.Instance.cheatSettings.oneHitKill)
        {
            //Todo : remettre cette ligne quand les anims serotn branchées
            //StartDeathAnimation();

            Die();
        }
    }


    public override void Cleanup()
    {
        OnHealthChanged = null;
        OnDamageTaken = null;
        OnHealed = null;
        OnDeathStart = null;
        OnDeath = null;
    }

    protected virtual void Die()
    {
        // this.Log($"{Owner} has died.");
        if (GameConfig.Instance.cheatSettings.npcImmortality)
            return;
        m_isDead = true;

        m_punchTween?.Kill(complete: true);

        LeanPool.Spawn(m_deathFxPefab, transform.position + Vector3.up * Owner.Height / 2f, Quaternion.Euler(0, Random.Range(0, 360), 0));

        SoundManager.Instance.PlaySound(SoundKeys.SFX_Groan);

        if (Owner.TryGetModule(out EntityTeamModule teamModule) && teamModule.Team == Team.Enemy)
            GameData.Instance.IncrementTrackedValue(TrackedValueType.EnemiesKilledThisRun);

        OnDeathStart?.Invoke();
        StartCoroutine(DeathAnimCR());
    }

    private IEnumerator DeathAnimCR()
    {
        Owner.Animator.Play("Death");
        m_customRE.ChangeFloat("_Saturation", 0f);
        if (m_ragdollManager != null)
            m_ragdollManager.EnableRagdoll();

        yield return new WaitForSeconds(m_deathDespawnDelay);

        OnDeath?.Invoke();
        m_customRE.ClearOverrides();
    }
}