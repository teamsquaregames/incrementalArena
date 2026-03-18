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

public class EntityHealthModule : EntityModule
{
    public Action<float, float, float> OnHealthChanged;
    public Action<float, float> OnDamageTaken;
    public Action<float, float> OnHealed;
    public Action OnDeathStart;
    public Action OnDeath;
    
    [Header("References")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private ParticleSystem m_deathFxPefab;
    
    [FoldoutGroup("Feedback settings")][SerializeField] private Vector3 punchScale = new Vector3(0.3f, -0.2f, 0f);
    [FoldoutGroup("Feedback settings")][SerializeField, Min(0f)] private float punchDuration = 0.35f;
    [FoldoutGroup("Feedback settings")][SerializeField, Min(1)] private int punchVibrato = 6;
    [FoldoutGroup("Feedback settings")][SerializeField, Range(0f, 1f)] private float punchElasticity = 0.5f;

    private float m_currentHealth;
    private bool m_isDead;
    private Tween m_punchTween;

    public float MaxHealth
    {
        get
        {
            if (Owner.TryGetModule(out EntityStatModule statModule))
            {
                return statModule.GetValue(StatType.MaxHealth);
            }
            else
            {
                this.LogWarning("No StatModule attached. Couldn't get MaxHealth value. Returning 100 as default");
                return 100;
                
            }
        }
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        m_isDead = false;
        m_animator = GetComponentInChildren<Animator>();
    }

    public override void OnAllModuleInitialized()
    {
        base.OnAllModuleInitialized();
        m_currentHealth = MaxHealth;
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(Random.Range(5, 20), false);
        }
    }

    private void PlayDamageFeedback()
    {
        PlayPunchScale();

        if (Owner.TryGetModule(out EntitySheenModule sheenModule))
        {
            sheenModule.PlayWhiteSheen();
        }

        if (m_animator != null)
        {
            m_animator.SetTrigger("Damage");
        }
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

    [Button]
    public void TakeDamage(float amount, bool isCrit)
    {
        if (m_isDead || amount <= 0f) return;

        float previous = m_currentHealth;
        m_currentHealth = Mathf.Max(0f, m_currentHealth - amount);
        float delta = m_currentHealth - previous;

        string amountText = "";
        if (isCrit)
        {
            amountText += "<sprite=\"crit\" name=\"crit\"> ";
        }
        amountText += amount.ToString("N0");

        PlayDamageFeedback();

        OnDamageTaken?.Invoke(amount, m_currentHealth);
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, delta);

        if (m_currentHealth <= 0f)
        {
            //Todo : remettre cette ligne quand les anims serotn branchées
            //StartDeathAnimation();

            Die();
        }
    }

    private void StartDeathAnimation()
    {
        if (m_isDead) return;
        m_isDead = true;

        OnDeathStart?.Invoke();
    }

    public void Die()
    {
        m_punchTween?.Kill(complete: true);
        
        LeanPool.Spawn(m_deathFxPefab,transform.position, Quaternion.identity);

        OnDeath?.Invoke();
    }
}