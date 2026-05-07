using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class RagdollManager : MonoBehaviour
{
    [TitleGroup("Dependencies")]
    [SerializeField, Required] private Entity m_owner;
    [SerializeField, Required] private Rigidbody[] m_ragdollRigidbodies;
    [SerializeField, Required] private Collider[] m_ragdollColliders;

    [Button]
    public void CacheReferences()
    {
        m_owner = GetComponent<Entity>();
        m_ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        m_ragdollColliders = GetComponentsInChildren<Collider>();
    }

    public void EnableRagdoll()
    {
        this.Log("Enabling ragdoll for " + m_owner.name);
        m_owner.Animator.enabled = false;

        foreach (var col in m_ragdollColliders)
        {
            col.enabled = true;
        }
    }

    [Button]
    public void DisableRagdoll()
    {
        this.Log("Disabling ragdoll for " + m_owner.name);
        foreach (var rb in m_ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }

        foreach (var col in m_ragdollColliders)
        {
            col.enabled = false;
        }

        m_owner.Animator.enabled = true;
    }
}
