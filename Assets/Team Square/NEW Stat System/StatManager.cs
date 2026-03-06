using System;
using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Stats
{
    public class StatManager : Singleton<StatManager>
    {
        [SerializeField] private List<EntityStatDefinition> m_entityStatDefinitions;

        [SerializeField, ReadOnly] private SerializableDictionary<EntityType, EntityStatDefinition> m_definitionLookup;
        [SerializeField, ReadOnly] private SerializableDictionary<EntityType, SerializableDictionary<StatType, Stat>> m_definitionStats;
        [SerializeField, ReadOnly] private SerializableDictionary<string, SerializableDictionary<StatType, Stat>> m_instanceStats;
        [SerializeField, ReadOnly] private SerializableDictionary<string, EntityType> m_instanceEntityTypes;

        protected void Awake()
        {
            m_definitionLookup    = new SerializableDictionary<EntityType, EntityStatDefinition>();
            m_definitionStats     = new SerializableDictionary<EntityType, SerializableDictionary<StatType, Stat>>();
            m_instanceStats       = new SerializableDictionary<string, SerializableDictionary<StatType, Stat>>();
            m_instanceEntityTypes = new SerializableDictionary<string, EntityType>();

            foreach (var definition in m_entityStatDefinitions)
            {
                if (definition == null) continue;
                m_definitionLookup[definition.entityType] = definition;
                m_definitionStats[definition.entityType]  = new SerializableDictionary<StatType, Stat>();

                foreach (var (type, baseValue) in definition.baseValues)
                    m_definitionStats[definition.entityType][type] = new Stat(baseValue);
            }
        }

        // --- Definition access (skill tree, no spawn needed) ---

        public Stat GetDefinitionStat(EntityType entityType, StatType statType)
        {
            if (!m_definitionStats.TryGetValue(entityType, out var stats))
            {
                stats = new SerializableDictionary<StatType, Stat>();
                m_definitionStats[entityType] = stats;
            }

            if (!stats.TryGetValue(statType, out var stat))
            {
                stat = new Stat();
                stats[statType] = stat;
            }

            return stat;
        }

        public float GetDefinitionValue(EntityType entityType, StatType statType)
        {
            return GetDefinitionStat(entityType, statType).Value;
        }

        public void AddDefinitionModifier(EntityType entityType, StatModifier mod)
        {
            GetDefinitionStat(entityType, mod.statType).AddModifier(mod);
        }

        public void RemoveDefinitionModifier(EntityType entityType, StatModifier mod)
        {
            GetDefinitionStat(entityType, mod.statType).RemoveModifier(mod);
        }

        // --- Instance access (spawned units) ---

        public string RegisterInstance(EntityType entityType)
        {
            var id    = $"{entityType}_{Guid.NewGuid():N}";
            var stats = new SerializableDictionary<StatType, Stat>();

            if (m_definitionStats.TryGetValue(entityType, out var defStats))
            {
                foreach (var (type, defStat) in defStats)
                {
                    var instanceStat = new Stat(defStat.Value);
                    defStat.OnValueChanged += instanceStat.SetBaseValue;
                    stats[type] = instanceStat;
                }
            }

            m_instanceStats[id]       = stats;
            m_instanceEntityTypes[id] = entityType;
            return id;
        }

        public void UnregisterInstance(string id)
        {
            if (!m_instanceStats.TryGetValue(id, out var stats)) return;

            if (m_instanceEntityTypes.TryGetValue(id, out var entityType) &&
                m_definitionStats.TryGetValue(entityType, out var defStats))
            {
                foreach (var (type, instanceStat) in stats)
                {
                    if (defStats.TryGetValue(type, out var defStat))
                        defStat.OnValueChanged -= instanceStat.SetBaseValue;
                }
            }

            m_instanceStats.Remove(id);
            m_instanceEntityTypes.Remove(id);
        }

        public Stat GetInstanceStat(string id, StatType statType)
        {
            if (!m_instanceStats.TryGetValue(id, out var stats))
            {
                stats = new SerializableDictionary<StatType, Stat>();
                m_instanceStats[id] = stats;
            }

            if (!stats.TryGetValue(statType, out var stat))
            {
                stat = new Stat();
                stats[statType] = stat;
            }

            return stat;
        }

        public float GetInstanceValue(string id, StatType statType)
        {
            return GetInstanceStat(id, statType).Value;
        }

        public void AddInstanceModifier(string id, StatModifier mod)
        {
            GetInstanceStat(id, mod.statType).AddModifier(mod);
        }

        public void RemoveInstanceModifier(string id, StatModifier mod)
        {
            GetInstanceStat(id, mod.statType).RemoveModifier(mod);
        }
    }
}