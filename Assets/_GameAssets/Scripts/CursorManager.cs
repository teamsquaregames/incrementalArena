using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] private SpriteRenderer m_cursorSprite;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private float m_defaultCursorSize = 4;

    private Vector3 m_mouseWorldPosition;
    [SerializeField,ReadOnly] private List<Entity> m_entitiesInCursor;

    public Vector3 MouseWorldPosition => m_mouseWorldPosition;
    public List<Entity> EntitiesInCursor => m_entitiesInCursor;

    private void Awake()
    {
        SetCursorSize(m_defaultCursorSize);
    }

    private void Update()
    {
        Ray ray = CameraManager.Instance.MainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_groundLayer))
        {
            m_mouseWorldPosition = hit.point;
            m_cursorSprite.transform.position = m_mouseWorldPosition.OffsetY(0.05f);
        }
        
        GetEntitiesInCursor();
    }
    
    private void GetEntitiesInCursor()
    {
        m_entitiesInCursor = new List<Entity>();
        Collider[] colliders = Physics.OverlapSphere(m_mouseWorldPosition, m_cursorSprite.transform.localScale.x / 2);
        foreach (var collider in colliders)
        {
            if (EntityManager.Instance.EntitiesByCollider.TryGetValue(collider, out Entity entity))
            {
                m_entitiesInCursor.Add(entity);

                if (entity.TryGetModule(out EntitySheenModule sheenModule))
                {
                    sheenModule.PlayWhiteSheen();
                }
            }
        }
    }

    public void SetCursorSize(float cursorSize)
    {
        m_cursorSprite.transform.localScale = Vector3.one * cursorSize;
    }
}