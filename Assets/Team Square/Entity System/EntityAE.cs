using UnityEngine;

public class EntityAE : MonoBehaviour
{
    private Entity m_entity;

    private void Awake()
    {
        m_entity = GetComponentInParent<Entity>();
    }
}