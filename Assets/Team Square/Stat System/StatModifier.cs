using System;

namespace Stats
{
    public enum ModifierType
    {
        Flat,
        Multiplier
    }

    [Serializable]
    public class StatModifier
    {
        public string id;
        public EntityType entityType;
        public StatType statType;
        public float value;
        public ModifierType type;

        public StatModifier(EntityType _entityType, StatType _statType, float _value, ModifierType _type, string _id = null)
        {
            id         = _id;
            entityType = _entityType;
            statType   = _statType;
            value      = _value;
            type       = _type;
        }

        public StatModifier Copy() => new StatModifier(entityType, statType, value, type, id);
    }
}