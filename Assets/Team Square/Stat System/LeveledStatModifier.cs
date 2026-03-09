using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats
{
    [Serializable]
    public class LeveledStatModifier : AStatModifier
    {
        [ListDrawerSettings(HideAddButton = true)]
        [SerializeField] private float[] m_values;

        public LeveledStatModifier(EntityType _entityType, StatType _statType, ModifierType _type, float[] _values, string _id = null)
            : base(_entityType, _statType, _type, _id)
        {
            m_values = _values;
        }

        public StatModifier GetModifierAtLevel(int level)
        {
            if (m_values == null || m_values.Length == 0)
            {
                Debug.LogWarning($"LeveledStatModifier '{id}' has no values defined. Returning modifier with value 0.");
                return new StatModifier(entityType, statType, 0, ModifierType.Flat, id);
            }

            if (level < 0 || level >= m_values.Length)
            {
                Debug.LogWarning($"Level {level} is out of range [0, {m_values.Length - 1}] for LeveledStatModifier '{id}'. Returning modifier with value 0.");
                return new StatModifier(entityType, statType, 0, ModifierType.Flat, id);
            }

            return new StatModifier(entityType, statType, m_values[level], type, id);
        }
        
        public void ResizeValues(int newSize)
        {
            if (newSize < 0) newSize = 0;

            var oldValues = m_values ?? Array.Empty<float>();
            var resized   = new float[newSize];

            for (int i = 0; i < Mathf.Min(oldValues.Length, newSize); i++)
                resized[i] = oldValues[i];

            m_values = resized;
        }
    }
}