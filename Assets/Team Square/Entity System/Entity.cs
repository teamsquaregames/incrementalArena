using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class Entity : MonoBehaviour, IPoolable
{
    [TitleGroup("References")]
    [SerializeField, Required] private Animator m_animator;
    [SerializeField, Required] private Collider m_collider;

    [FoldoutGroup("Status"), SerializeField] private AnimationCurve m_knockUpCurve;
    [FoldoutGroup("Status"), SerializeField] private Transform m_modelT;


    [TitleGroup("Settings")]
    [SerializeField] private EntityType m_entityType;

    private Dictionary<Type, EntityModule> m_modules = new Dictionary<Type, EntityModule>();
    private bool m_isStaggered;
    private float m_staggerEndTime;
    private Coroutine m_staggerCR;
    private Coroutine m_knockUpCR;
    private bool m_isSpawning;

    public EntityType EntityType => m_entityType;
    public Animator Animator => m_animator;
    public Collider Collider => m_collider;
    public bool IsStaggered => m_isStaggered;
    public bool IsSpawning => m_isSpawning;

    internal void SetSpawning(bool value) => m_isSpawning = value;



    [Button]
    public void CacheReferences()
    {
        m_collider = GetComponent<Collider>();
        m_animator = GetComponentInChildren<Animator>();
    }

    private void RegisterModules()
    {
        var modules = GetComponents<EntityModule>();
        foreach (var module in modules)
        {
            var type = module.GetType();
            while (type != null && typeof(EntityModule).IsAssignableFrom(type))
            {
                m_modules.TryAdd(type, module);
                type = type.BaseType;
            }

            module.Initialize(this);
        }

        foreach (var module in m_modules.Values.Distinct())
        {
            module.OnAllModuleInitialized();
        }

        if (TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeathStart += Unregister;
            healthModule.OnDeath += Despawn;
        }
    }


    public bool TryGetModule<T>(out T module) where T : EntityModule
    {
        if (m_modules.TryGetValue(typeof(T), out var raw))
        {
            module = (T)raw;
            return true;
        }

        module = null;
        return false;
    }

    public void Despawn()
    {
        if (TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeathStart -= Unregister;
            healthModule.OnDeath -= Despawn;
        }
        
        LeanPool.Despawn(this);
    }

    private void Register()
    {
        m_collider.enabled = true;
        EntityManager.Instance?.Register(this);
    }

    private void Unregister()
    {
        m_collider.enabled = false;
        EntityManager.Instance?.Unregister(this);
    }

    public void OnSpawn()
    {
        // this.Log($"Spawning entity {name} of type {EntityType}");
        RegisterModules();
        Register();
    }

    private void OnEnable()
    {
        ResetStaggerState();
    }

    private void ResetStaggerState()
    {
        m_isStaggered = false;
        m_staggerEndTime = 0f;

        if (m_staggerCR != null)
        {
            StopCoroutine(m_staggerCR);
            m_staggerCR = null;
        }

        if (m_knockUpCR != null)
        {
            StopCoroutine(m_knockUpCR);
            m_knockUpCR = null;
        }

        m_modelT.localPosition = Vector3.zero;
    }

    public void OnDespawn()
    {
        foreach (var module in m_modules.Values.Distinct())
            module.Cleanup();
    }


    public void Stagger(float duration)
    {
        if (!isActiveAndEnabled || (TryGetModule(out EntityHealthModule healthModule) && healthModule.IsDead) || duration <= 0f)
            return;

        m_isStaggered = true;

        if (m_staggerCR != null)
        {
            m_staggerEndTime += duration;
        }
        else
        {
            m_staggerEndTime = Time.time + duration;
            m_staggerCR = StartCoroutine(StaggerCR());
        }

        // int staggerVariation = (int)math.round(UnityEngine.Random.Range(0f, 2f));
        m_animator.SetFloat("StaggerVariation", UnityEngine.Random.Range(0f, 1f));
        m_animator.Play("Staggered");
    }

    public void Stun(float duration)
    {
        this.LogWarning($"PREVIEW! Need more implementations. Entity {name} is stunned for {duration} seconds.");
        Stagger(duration);
    }

    public void KnockUp(float duration)
    {
        if (!isActiveAndEnabled || duration <= 0f)
            return;

        Stagger(duration);

        if (m_knockUpCR != null)
        {
            StopCoroutine(m_knockUpCR);
        }
        m_knockUpCR = StartCoroutine(KnockUpCR(duration));
    }

    private IEnumerator StaggerCR()
    {
        while (Time.time < m_staggerEndTime)
        {
            yield return null;
        }

        m_isStaggered = false;
        m_staggerCR = null;
    }

    private IEnumerator KnockUpCR(float duration)
    {
        float knockUpStartTime = Time.time;

        while (m_isStaggered)
        {
            float elapsed = Time.time - knockUpStartTime;
            float height = m_knockUpCurve.Evaluate(elapsed / duration);
            m_modelT.localPosition = Vector3.zero + Vector3.up * height;
            yield return new WaitForEndOfFrame();
        }
    }
}