using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats
{
    
    public enum ModifierType
    {
        Flat,
        Percentage,
        Multiplier
    }
    
    [Serializable]
    public abstract class AStatModifier
    {
        public string id;
        // [ShowIf("ability", null)]
        public EntityType entityType;
        public AbilityConfig ability;
        [HorizontalGroup, ShowIf("ability", "!null"), Range(0, 5)]
        public int step = 0;
        [HorizontalGroup, ShowIf("ability", "!null"), Range(0, 5)]  
        public int application = 0;
        public StatType statType;
        public ModifierType type;

        protected AStatModifier(EntityType _entityType, StatType _statType, ModifierType _type, string _id = null)
        {
            id         = _id;
            entityType = _entityType;
            statType   = _statType;
            type       = _type;
        }
    }
}
