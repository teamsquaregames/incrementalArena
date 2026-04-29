using Unity.VisualScripting;
using UnityEngine;
using Utils.Playable;

public class EntityAE : MonoBehaviour
{
    private Entity m_entity;
    private EntityAbilityModule m_abilityModule;
    private EntitySpawnModule m_spawnModule;

    private IPlayable[] m_playables;


    private void Awake()
    {
        m_entity = GetComponentInParent<Entity>();
        m_abilityModule = GetComponentInParent<EntityAbilityModule>();
        m_spawnModule = GetComponentInParent<EntitySpawnModule>();
        m_playables = GetComponentsInChildren<IPlayable>();
    }


    public void OnAbilityStart()
    {
        m_abilityModule?.HandleAnimationStart();
    }

    public void HandleAnimationStart()
    {
        OnAbilityStart();
    }

    public void OnAbilityActive()
    {
        m_abilityModule?.HandleAnimationActive();
    }

    public void HandleAnimationActive()
    {
        OnAbilityActive();
    }

    public void OnAbilityEnd()
    {
        m_abilityModule?.HandleAnimationEnd();
    }

    public void HandleAnimationEnd()
    {
        OnAbilityEnd();
    }

    public void OnAbilityInterrupt()
    {
        m_abilityModule?.HandleAnimationInterrupt();
    }

    public void HandleAnimationInterrupt()
    {
        OnAbilityInterrupt();
    }

    public void OnSpawnEnd()
    {
        m_spawnModule?.HandleSpawnEnd();
    }

    public void OnFootStep()
    {
        foreach (IPlayable playable in m_playables)
        {
            if ((playable.PlayFlags & PlayFlags.OnFootStep) != 0)
                playable.Play();
        }
    }
}