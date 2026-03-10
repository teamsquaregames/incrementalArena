using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats
{
    public class EntityStatModule : EntityModule
    {
        [SerializeField] private EntityType m_entityType;

        private void Awake()
        {
            StatManager.Instance.RegisterInstance(gameObject, m_entityType);
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