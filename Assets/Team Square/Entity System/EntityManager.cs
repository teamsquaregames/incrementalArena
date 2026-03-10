using System;
using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityManager : Singleton<EntityManager>
{
    public Action<Entity> onEntityRegistered;
    public Action<Entity> onEntityUnregistered;
    
    [SerializeField, ReadOnly]private Entity m_player;
    [SerializeField, ReadOnly] private List<Entity> m_enemies = new List<Entity>();
    [SerializeField, ReadOnly] private List<Entity> m_entities = new List<Entity>();
    private Dictionary<Collider, Entity> m_entitiesByColliders = new Dictionary<Collider, Entity>();
    
    public List<Entity> AllEntities => m_entities;
    public Dictionary<Collider, Entity> EntitiesByCollider => m_entitiesByColliders;
    public List<Entity> Enemies => m_enemies;
    public Entity Player => m_player;
    
    internal void Register(Entity entity)
    {
        if (entity == null) return;

        if (!m_entities.Contains(entity))
        {
            m_entities.Add(entity);
        }

        if (!m_entitiesByColliders.ContainsKey(entity.Collider))
        {
            m_entitiesByColliders.Add(entity.Collider, entity);
        }

        if (entity.TryGetModule(out EntityTeamModule teamModule))
        {
            if (teamModule.Team == Team.Player)
            {
                m_player = entity;
            }
            else if (teamModule.Team == Team.Enemy)
            {
                m_enemies.Add(entity);
            }
        }
        
        onEntityRegistered?.Invoke(entity);
    }

    internal void Unregister(Entity entity)
    {
        if (entity == null) return;
        
        m_entities.Remove(entity);
        m_entitiesByColliders.Remove(entity.Collider);
        if (entity.TryGetModule(out EntityTeamModule teamModule))
        {
            if (teamModule.Team == Team.Player)
            {
                m_player = entity;
            }
            else if (teamModule.Team == Team.Enemy)
            {
                m_enemies.Remove(entity);
            }
        }
        
        onEntityUnregistered?.Invoke(entity);
    }

    public Entity FindClosestEnemy(Entity sourceEntity)
    {
        Entity closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in m_entities)
        {
            if (entity != sourceEntity && entity.IsEnemy(sourceEntity))
            {
                float distance = Vector3.Distance(sourceEntity.transform.position, entity.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = entity;
                }
            }
        }

        return closest;
    }
}