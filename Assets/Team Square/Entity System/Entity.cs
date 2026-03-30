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
    [SerializeField] private Collider m_collider;

    private Dictionary<Type, EntityModule> m_modules = new Dictionary<Type, EntityModule>();

    public Collider Collider => m_collider;

    [FoldoutGroup("Status")]
    public bool IsStunned => m_isStunned;
    [FoldoutGroup("Status")]
    [SerializeField] private AnimationCurve m_knockUpCurve;
    [FoldoutGroup("Status")]
    [SerializeField] private Transform m_modelT;
    private bool m_isStunned;
    private float m_stunEndTime;
    private Coroutine m_stunCR;
    private Coroutine m_knockUpCR;


    [Button]
    public void CacheReferences()
    {
        m_collider = GetComponent<Collider>();
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

        foreach (var module in m_modules.Values)
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

    private void Despawn()
    {
        if (TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeathStart -= Unregister;
            healthModule.OnDeath -= Despawn;
        }

        Unregister();
        LeanPool.Despawn(gameObject);
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
        RegisterModules();
        Register();
    }

    public void OnDespawn()
    {
    }


    public async void Stun(float duration, bool knockUp = false)
    {
        this.LogWarning($"PREVIEW! Need more implementations. Entity {name} is stunned for {duration} seconds.");
        m_isStunned = true;

        if (m_stunCR != null)
        {
            m_stunEndTime = m_stunEndTime + duration;
        }
        else
        {
            m_stunEndTime = Time.time + duration;
            m_stunCR = StartCoroutine(StunCR(duration));
        }

        if (knockUp)
        {
            if (m_knockUpCR != null)
            {
                StopCoroutine(m_knockUpCR);
            }
            m_knockUpCR = StartCoroutine(KnockUpCR(duration));
        }
    }

    private IEnumerator StunCR(float duration)
    {
        while (Time.time < m_stunEndTime)
        {
            yield return null;
        }

        m_isStunned = false;
        m_stunCR = null;
    }

    private IEnumerator KnockUpCR(float duration)
    {
        float knockUpStartTime = Time.time;

        while (m_isStunned)
        {
            float elapsed = Time.time - knockUpStartTime;
            float height = m_knockUpCurve.Evaluate(elapsed / duration);
            m_modelT.position = Vector3.zero + Vector3.up * height;
            yield return new WaitForEndOfFrame();
        }
    }
}