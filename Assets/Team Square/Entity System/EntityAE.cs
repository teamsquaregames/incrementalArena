using Unity.VisualScripting;
using UnityEngine;
using Utils.Playable;

public class EntityAE : MonoBehaviour
{
    private Entity m_entity;
    private EntityAbilityModule m_abilityModule;

    private IPlayable[] m_playables;


    private void Awake()
    {
        m_entity = GetComponentInParent<Entity>();
        m_abilityModule = GetComponentInParent<EntityAbilityModule>();
        m_playables = GetComponentsInChildren<IPlayable>();
    }


    public void OnAbilityStart()
    {
        m_abilityModule?.HandleAnimationStart();
    }

    public void OnAbilityActive()
    {
        m_abilityModule?.HandleAnimationActive();
    }

    public void OnAbilityEnd()
    {
        m_abilityModule?.HandleAnimationEnd();
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