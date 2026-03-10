using System;
using System.Collections.Generic;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider m_collider;
    
    private Dictionary<Type, EntityModule> m_modules = new Dictionary<Type, EntityModule>();
    
    public Collider Collider => m_collider;
    
    [Button]
    public void CacheReferences()
    {
        m_collider = GetComponent<Collider>();
    }

    private void Awake()
    {
        RegisterModules();
        Register();
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

    public bool IsEnemy(Entity entity)
    {
        if (TryGetModule(out EntityTeamModule myTeamModule) && entity.TryGetModule(out EntityTeamModule otherTeamModule))
        {
            return otherTeamModule.EnemyTeam == myTeamModule.Team;
        }
        
        return false;
    }

    private void Despawn()
    {
        if (TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeathStart -= Unregister;
            healthModule.OnDeath -= Despawn;
        }
        
        Destroy(gameObject);
    }

    private void Register()
    {
        m_collider.enabled = true;
        EntityManager.Instance?.Register(this);
    }

    private void Unregister()
    {
        m_collider.enabled = false;
        if (EntityManager.Instance != null)
        {
            EntityManager.Instance?.Unregister(this);
        }
    }

    private void OnDestroy()
    {
        Unregister();
    }
}