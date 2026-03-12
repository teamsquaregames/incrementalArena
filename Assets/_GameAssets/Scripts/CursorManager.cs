using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] private SpriteRenderer m_cursorSprite;
    [SerializeField] private LayerMask m_groundLayer;

    private Vector3 m_mouseWorldPosition;

    public Vector3 MouseWorldPosition => m_mouseWorldPosition;

    private void Update()
    {
        Ray ray = CameraManager.Instance.MainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_groundLayer))
        {
            m_mouseWorldPosition = hit.point;
            m_cursorSprite.transform.position = m_mouseWorldPosition.OffsetY(0.05f);
        }
    }
}