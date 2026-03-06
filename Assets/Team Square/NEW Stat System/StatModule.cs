using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats
{
    public class StatModule : MonoBehaviour
    {
        [SerializeField] private EntityType m_entityType;
        [SerializeField, ReadOnly] private string m_instanceId;

        public string InstanceId => m_instanceId;

        private void Awake()
        {
            m_instanceId = StatManager.Instance.RegisterInstance(m_entityType);
        }

        private void OnDestroy()
        {
            if (StatManager.Instance != null)
            {
                StatManager.Instance.UnregisterInstance(m_instanceId);
            }
        }

        public float GetValue(StatType type)         => StatManager.Instance.GetInstanceValue(m_instanceId, type);
        public void AddModifier(StatModifier mod)    => StatManager.Instance.AddInstanceModifier(m_instanceId, mod);
        public void RemoveModifier(StatModifier mod) => StatManager.Instance.RemoveInstanceModifier(m_instanceId, mod);
    }
}