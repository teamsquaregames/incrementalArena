using System;
using Sirenix.OdinInspector;

namespace Stats
{
    
    public enum ModifierType
    {
        Flat,
        AdditivePercentage,
        Multiplier
    }
    
    [Serializable]
    public abstract class AStatModifier
    {
        public string id;
        // [ShowIf("ability", null)]
        public EntityType entityType;
        public AbilityConfig ability;
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
