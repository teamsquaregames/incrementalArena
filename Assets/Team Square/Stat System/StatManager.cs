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
        [SerializeField, AssetList(Path = "Team Square/Stat System", AutoPopulate = true)] private List<EntityStatDefinition> m_entityStatDefinitions;
        [SerializeField, ReadOnly] private SerializableDictionary<EntityType, Dictionary<StatType, Stat>> m_definitionStats;
        [SerializeField, ReadOnly] private SerializableDictionary<GameObject, Dictionary<StatType, Stat>> m_instanceStats;

        private SerializableDictionary<GameObject, EntityType> m_instanceEntityTypes;

        protected void Awake()
        {
            m_definitionStats     = new SerializableDictionary<EntityType, Dictionary<StatType, Stat>>();
            m_instanceStats       = new SerializableDictionary<GameObject, Dictionary<StatType, Stat>>();
            m_instanceEntityTypes = new SerializableDictionary<GameObject, EntityType>();

            foreach (var definition in m_entityStatDefinitions)
            {
                if (definition == null) continue;
                m_definitionStats[definition.entityType] = BuildStatDictionary(definition);
            }
        }

        private Dictionary<StatType, Stat> BuildStatDictionary(EntityStatDefinition definition)
        {
            var stats = new Dictionary<StatType, Stat>();
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                var baseValue = definition.baseValues.TryGetValue(statType, out var v) ? v : 0f;
                stats[statType] = new Stat(baseValue);
            }
            return stats;
        }

        // --- Definition access (skill tree, no spawn needed) ---

        public Stat GetDefinitionStat(EntityType entityType, StatType statType)
        {
            return m_definitionStats[entityType][statType];
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

        public void RegisterInstance(GameObject owner, EntityType entityType)
        {
            var stats = new Dictionary<StatType, Stat>();

            if (m_definitionStats.TryGetValue(entityType, out var defStats))
            {
                foreach (var (type, defStat) in defStats)
                {
                    var instanceStat = new Stat(defStat.Value);
                    _ = instanceStat.Value; // force m_cachedValue to populate immediately
                    defStat.OnValueChanged += instanceStat.SetBaseValueAndRecalculate;
                    stats[type] = instanceStat;
                }
            }

            m_instanceStats[owner]       = stats;
            m_instanceEntityTypes[owner] = entityType;
        }

        public void UnregisterInstance(GameObject owner)
        {
            if (!m_instanceStats.TryGetValue(owner, out var stats)) return;

            if (m_instanceEntityTypes.TryGetValue(owner, out var entityType) &&
                m_definitionStats.TryGetValue(entityType, out var defStats))
            {
                foreach (var (type, instanceStat) in stats)
                {
                    if (defStats.TryGetValue(type, out var defStat))
                        defStat.OnValueChanged -= instanceStat.SetBaseValueAndRecalculate;
                }
            }

            m_instanceStats.Remove(owner);
            m_instanceEntityTypes.Remove(owner);
        }

        public Stat GetInstanceStat(GameObject owner, StatType statType)
        {
            return m_instanceStats[owner][statType];
        }

        public float GetInstanceValue(GameObject owner, StatType statType)
        {
            return GetInstanceStat(owner, statType).Value;
        }

        public void AddInstanceModifier(GameObject owner, StatModifier mod)
        {
            GetInstanceStat(owner, mod.statType).AddModifier(mod);
        }

        public void RemoveInstanceModifier(GameObject owner, StatModifier mod)
        {
            GetInstanceStat(owner, mod.statType).RemoveModifier(mod);
        }

        #region Debug
        [SerializeField] private EntityType m_debugEntityType;
        [SerializeField] private StatModifier m_statModifier;

        [Button]
        public void AddModifier()
        {
            AddDefinitionModifier(m_debugEntityType, m_statModifier);
        }

        [Button]
        public void RefreshAllStats()
        {
            foreach (var statDict in m_definitionStats.Values)
                foreach (var stat in statDict.Values)
                    stat.ForceRecalculate();

            foreach (var statDict in m_instanceStats.Values)
                foreach (var stat in statDict.Values)
                    stat.ForceRecalculate();
        }
        #endregion
    }
}