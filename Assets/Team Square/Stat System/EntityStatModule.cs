using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Stats
{
    public class EntityStatModule : EntityModule
    {

        protected override void OnInitialize()
        {
            // this.Log($"Initializing EntityStatModule for {Owner.name}");
            base.OnInitialize();

            StatManager.Instance.RegisterInstance(gameObject, Owner.EntityType);
        }

        private void OnDestroy()
        {
            if (StatManager.Instance != null)
                StatManager.Instance.UnregisterInstance(gameObject);
        }

        [Button]
        public float GetValue(StatType type)         => StatManager.Instance.GetInstanceValue(gameObject, type);
        public void AddModifier(StatModifier mod)    => StatManager.Instance.AddInstanceModifier(gameObject, mod);
        public void RemoveModifier(StatModifier mod) => StatManager.Instance.RemoveInstanceModifier(gameObject, mod);
    }
}