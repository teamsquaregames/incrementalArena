using System;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class EntityHealthModule : EntityModule
{
    public Action<float, float, float> OnHealthChanged;
    public Action<float, float> OnDamageTaken;
    public Action<float, float> OnHealed;
    public Action OnDeathStart;
    public Action OnDeath;
    
    [Header("References")]
    [SerializeField] private ParticleSystemPoolRef m_deathFxPoolRef;
    
    [FoldoutGroup("Feedback settings")][SerializeField] private Vector3 punchScale = new Vector3(0.3f, -0.2f, 0f);
    [FoldoutGroup("Feedback settings")][SerializeField, Min(0f)] private float punchDuration = 0.35f;
    [FoldoutGroup("Feedback settings")][SerializeField, Min(1)] private int punchVibrato = 6;
    [FoldoutGroup("Feedback settings")][SerializeField, Range(0f, 1f)] private float punchElasticity = 0.5f;

    private float m_currentHealth;
    private bool m_isDead;
    private Tween m_punchTween;

    public float MaxHealth => 100; //Todo : replace by stat

    protected override void OnInitialize()
    {
        m_currentHealth = MaxHealth;
        m_isDead = false;
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
            StartDeathAnimation();
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

        m_deathFxPoolRef.pool.Spawn(transform.position, Quaternion.identity, m_deathFxPoolRef.pool.transform);

        OnDeath?.Invoke();
    }
}