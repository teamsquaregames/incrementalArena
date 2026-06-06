using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Stats
{
    public class StatManager : Singleton<StatManager>
    {
        [SerializeField, AssetList(Path = "_GameAssets", AutoPopulate = true)] private List<EntityStatDefinition> m_entityStatDefinitions;

        [SerializeField, ReadOnly] private SerializableDictionary<EntityType, Dictionary<StatType, Stat>> m_definitionStats;
        [SerializeField, ReadOnly] private SerializableDictionary<AbilityConfig, Dictionary<StatType, Stat[,]>> m_definitionAbilityStats;
    
        [SerializeField, ReadOnly] private SerializableDictionary<GameObject, Dictionary<StatType, Stat>> m_instanceStats;
        [SerializeField, ReadOnly] private SerializableDictionary<GameObject, Dictionary<AbilityConfig, Dictionary<StatType, Stat[,]>>> m_instanceAbilityStats;

        private SerializableDictionary<GameObject, EntityType> m_instanceEntityTypes;

        protected void Awake()
        {
            // this.Log("Awakening StatManager and initializing stat dictionaries");
            m_definitionStats = new SerializableDictionary<EntityType, Dictionary<StatType, Stat>>();
            m_definitionAbilityStats = new SerializableDictionary<AbilityConfig, Dictionary<StatType, Stat[,]>>();
            m_instanceStats = new SerializableDictionary<GameObject, Dictionary<StatType, Stat>>();
            m_instanceAbilityStats = new SerializableDictionary<GameObject, Dictionary<AbilityConfig, Dictionary<StatType, Stat[,]>>>();

            m_instanceEntityTypes = new SerializableDictionary<GameObject, EntityType>();

            foreach (var definition in m_entityStatDefinitions)
            {
                //Debug.Log(definition.entityType, definition);
                if (definition == null) continue;
                m_definitionStats[definition.entityType] = BuildStatDictionary(definition);
            }
        }

        private Dictionary<StatType, Stat> BuildStatDictionary(EntityStatDefinition definition)
        {
            var stats = new Dictionary<StatType, Stat>();
            foreach (var (statType, baseValue) in definition.baseValues)
                stats[statType] = new Stat(baseValue);
            return stats;
        }

        private Stat GetOrCreateDefinitionStat(EntityType entityType, StatType statType, double baseValue = 0)
        {
            // this.Log($"Getting definition stat for entity type '{entityType}' and stat type '{statType}'");
            if (!m_definitionStats.TryGetValue(entityType, out var statDict))
            {
                // this.Log($"Creating new stat dictionary for entity type '{entityType}'");
                statDict = new Dictionary<StatType, Stat>();
                m_definitionStats[entityType] = statDict;
            }

            if (!statDict.TryGetValue(statType, out var stat))
            {
                // this.Log($"Creating new stat for entity type '{entityType}' and stat type '{statType}'");
                stat = new Stat(0f);
                statDict[statType] = stat;
            }

            return stat;
        }

        private Stat GetOrCreateDefinitionStat(AbilityConfig ability, StatType statType, int step, int application)
        {
            // this.Log($"Getting definition stat for ability '{ability.abilityName}' and stat type '{statType}' with step {step} and application {application}");
            if (!m_definitionAbilityStats.TryGetValue(ability, out var statDict))
            {
                statDict = new Dictionary<StatType, Stat[,]>();
                m_definitionAbilityStats[ability] = statDict;
                // this.Log($"Created new stat dictionary for ability '{ability.abilityName}'");
            }

            if (!statDict.TryGetValue(statType, out var stat))
            {
                stat = new Stat[5, 5];
                statDict[statType] = stat;
                // this.Log($"Created new stat for ability '{ability.abilityName}' and stat type '{statType}' with base value 0");
            }

            if (stat[step, application] == null)
            {
                stat[step, application] = new Stat(0f);
                // this.Log($"Initialized stat for ability '{ability.abilityName}', step {step}, application {application}, stat type '{statType}' with base value 0");
            }

            return stat[step, application];
        }

        private Stat GetOrCreateInstanceStat(GameObject owner, StatType statType)
        {
            if (!m_instanceStats.TryGetValue(owner, out var statDict))
            {
                statDict = new Dictionary<StatType, Stat>();
                m_instanceStats[owner] = statDict;
            }

            if (!statDict.TryGetValue(statType, out var stat))
            {
                var entityType = m_instanceEntityTypes[owner];
                var defStat = GetOrCreateDefinitionStat(entityType, statType);
                stat = new Stat(defStat.Value);
                defStat.OnValueChanged += stat.SetBaseValueAndRecalculate;
                statDict[statType] = stat;
            }

            return stat;
        }

        private Stat GetOrCreateInstanceStat(GameObject owner, AbilityConfig ability, StatType statType, int step, int application)
        {
            if (!m_instanceAbilityStats.TryGetValue(owner, out var abilityDict))
            {
                abilityDict = new Dictionary<AbilityConfig, Dictionary<StatType, Stat[,]>>();
                m_instanceAbilityStats[owner] = abilityDict;
            }

            if (!abilityDict.TryGetValue(ability, out var statDict))
            {
                statDict = new Dictionary<StatType, Stat[,]>();
                abilityDict[ability] = statDict;
            }

            if (!statDict.TryGetValue(statType, out var stat))
            {
                var defStat = GetOrCreateDefinitionStat(ability, statType, step, application);
                stat = new Stat[5, 5];
                stat[step, application] = new Stat(defStat.Value);
                defStat.OnValueChanged += stat[step, application].SetBaseValueAndRecalculate;
                statDict[statType] = stat;
            }

            return stat[step, application];
        }
        
        
        // --- Definition access (skill tree, no spawn needed) ---

        public Stat GetDefinitionStat(EntityType entityType, StatType statType, double baseValue = 0)
        {
            return GetOrCreateDefinitionStat(entityType, statType, baseValue);
        }

        public Stat GetDefinitionStat(AbilityConfig ability, StatType statType, int step = 0, int application = 0)
        {
            return GetOrCreateDefinitionStat(ability, statType, step, application);
        }

        public float GetDefinitionValue(EntityType entityType, StatType statType, double baseValue = 0)
        {
            return GetDefinitionStat(entityType, statType, baseValue).Value;
        }
        
        public float GetDefinitionValue(AbilityConfig ability, StatType statType, int step = 0, int application = 0)
        {
            return GetDefinitionStat(ability, statType, step, application).Value;
        }


        public void AddDefinitionModifier(EntityType entityType, StatModifier mod)
        {
            // this.Log($"Adding definition modifier for entity type '{entityType}' and stat type '{mod.statType}' with value {mod.value}");
            foreach (var flag in entityType.GetFlags())
            {
                GetDefinitionStat(flag, mod.statType).AddModifier(mod);
            }
        }
        public void RemoveDefinitionModifier(EntityType entityType, StatModifier mod)
        {
            foreach (var flag in entityType.GetFlags())
            {
                GetDefinitionStat(flag, mod.statType).RemoveModifier(mod);
            }
        }

        public void AddDefinitionModifier(AbilityConfig ability, StatModifier mod, int step = 0, int application = 0)
        {
            GetDefinitionStat(ability, mod.statType, step, application).AddModifier(mod);
        }

        public void RemoveDefinitionModifier(AbilityConfig ability, StatModifier mod, int step = 0, int application = 0)
        {
            GetDefinitionStat(ability, mod.statType, step, application).RemoveModifier(mod);
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

            m_instanceStats[owner] = stats;
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
            return GetOrCreateInstanceStat(owner, statType);
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
        [SerializeField] private StatModifier m_statModifier;

        [Button]
        public void AddModifier()
        {
            AddDefinitionModifier(m_statModifier.entityType, m_statModifier);
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