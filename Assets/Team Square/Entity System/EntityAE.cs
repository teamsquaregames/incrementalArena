using UnityEngine;

public class EntityAE : MonoBehaviour
{
    private Entity m_entity;
    private EntityAbilityModule m_abilityModule;

    private void Awake()
    {
        m_entity = GetComponentInParent<Entity>();
        m_abilityModule = GetComponentInParent<EntityAbilityModule>();
    }
    
    public void OnAbilityTrigger()
    {
        m_abilityModule?.HandleAnimationEvent();
    }
    
    public void OnAbilityStart()
    {
        m_abilityModule?.HandleAnimationStart();
    }

    public void OnAbilityEnd()
    {
        m_abilityModule?.HandleAnimationEnd();
    }
}