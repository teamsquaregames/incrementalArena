using System;
using System.Collections.Generic;

namespace Stats
{
    public class Stat
    {
        private float baseValue;
        private readonly List<StatModifier> modifiers = new();
        private bool isDirty = true;
        private float cachedValue;

        public float Value
        {
            get
            {
                if (isDirty) Recalculate();
                return cachedValue;
            }
        }

        public event Action<float> OnValueChanged;

        public Stat(float baseValue = 0f)
        {
            this.baseValue = baseValue;
        }

        public void AddModifier(StatModifier mod)
        {
            modifiers.Add(mod);
            MarkDirty();
        }

        public void RemoveModifier(StatModifier mod)
        {
            modifiers.Remove(mod);
            MarkDirty();
        }

        public void RemoveAllModifiersFromSource(object source)
        {
            modifiers.RemoveAll(m => m.source == source);
            MarkDirty();
        }

        private void MarkDirty()
        {
            isDirty = true;
            OnValueChanged?.Invoke(Value);
        }

        private void Recalculate()
        {
            float flat  = baseValue;
            float multi = 1f;

            foreach (var mod in modifiers)
            {
                if (mod.type == ModifierType.Flat)       flat  += mod.value;
                if (mod.type == ModifierType.Multiplier) multi *= mod.value;
            }

            cachedValue = flat * multi;
            isDirty = false;
        }
    }
}