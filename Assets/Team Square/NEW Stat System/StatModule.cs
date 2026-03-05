using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Stats
{
    public class StatModule : MonoBehaviour
    {
        [SerializeField] private SerializableDictionary<StatType, float> baseValues;

        private readonly Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

        private Stat Get(StatType type)
        {
            if (!stats.TryGetValue(type, out var stat))
            {
                var baseValue = baseValues.TryGetValue(type, out var v) ? v : 0f;
                stat          = new Stat(baseValue);
                stats[type]   = stat;
            }
            return stat;
        }

        public float GetValue(StatType type) => Get(type).Value;

        public void AddModifier(StatModifier mod) => Get(mod.statType).AddModifier(mod);

        public void RemoveModifier(StatModifier mod) => Get(mod.statType).RemoveModifier(mod);
    }
}