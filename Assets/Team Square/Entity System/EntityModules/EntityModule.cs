using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public abstract class EntityModule : MonoBehaviour
{
    private Entity m_ownerEntity;

    public Entity Owner => m_ownerEntity;
    
    internal void Initialize(Entity owner)
    {
        m_ownerEntity = owner;
        OnInitialize();
    }
    
    protected virtual void OnInitialize() { }
    public virtual void OnAllModuleInitialized() { }
    [Button] public virtual void CacheReferences(){}
}