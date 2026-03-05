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
        public StatType statType;
        public float value;
        public ModifierType type;
        public object source;

        public StatModifier(StatType _statType, float _value, ModifierType _type, object _source = null)
        {
            statType = _statType;
            value    = _value;
            type     = _type;
            source   = _source;
        }
    }
}