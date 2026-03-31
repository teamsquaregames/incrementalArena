using System.Collections;
using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Utils;
using DG.Tweening;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] private DecalProjector m_cursorWidget;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private float m_defaultCursorSize = 4;

    private Vector3 m_mouseWorldPosition;
    [SerializeField, ReadOnly] private List<Entity> m_entitiesInCursor;

    public Vector3 MouseWorldPosition => m_mouseWorldPosition;
    public List<Entity> EntitiesInCursor => m_entitiesInCursor;


    [TitleGroup("Highlighting")]
    private string m_highlightShaderProperty = "_Bright_Disolve";
    private Coroutine m_highlightCoroutine;
    private bool m_isCursorHighlighted = false;
    private Coroutine m_unhighlightCoroutine;
    private Tween m_cursorSizeTween;
    private Tween m_highlightShaderTween;


    private void Awake()
    {
        SetCursorSize(m_defaultCursorSize);
        SetHighlightShaderValue(2);
    }

    private void Update()
    {
        Ray ray = CameraManager.Instance.MainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_groundLayer))
        {
            m_mouseWorldPosition = hit.point;
            m_cursorWidget.transform.position = m_mouseWorldPosition.OffsetY(0.05f);
        }

        GetEntitiesInCursor();
    }

    private void GetEntitiesInCursor()
    {
        m_entitiesInCursor = new List<Entity>();
        Collider[] colliders = Physics.OverlapSphere(m_mouseWorldPosition, m_cursorWidget.transform.localScale.x / 2);
        foreach (var collider in colliders)
        {
            if (EntityManager.Instance.EntitiesByCollider.TryGetValue(collider, out Entity entity))
            {
                m_entitiesInCursor.Add(entity);
            }
        }
        if (m_entitiesInCursor.Count > 1 || (m_entitiesInCursor.Count == 1 && m_entitiesInCursor[0].TryGetModule(out PlayerBrainModule _) == false))
        {
            if (!m_isCursorHighlighted)
            {
                if (m_unhighlightCoroutine != null)
                {
                    StopCoroutine(m_unhighlightCoroutine);
                }   
                m_highlightCoroutine = StartCoroutine(HighlightCursor());
            }
        }
        else if (m_isCursorHighlighted)
        {
            if (m_highlightCoroutine != null)
            {
                StopCoroutine(m_highlightCoroutine);
            }
            m_unhighlightCoroutine = StartCoroutine(UnhighlightCursor());
        }
    }

    public void SetCursorSize(float cursorSize)
    {
        m_cursorWidget.transform.localScale = Vector3.one * cursorSize;
        m_cursorWidget.size = new Vector3(cursorSize, cursorSize, 5f);
    }

    private IEnumerator HighlightCursor()
    {
        this.Log("Highlighting cursor");
        m_isCursorHighlighted = true;
        m_cursorWidget.gameObject.SetActive(true);

        m_cursorSizeTween?.Kill();
        m_highlightShaderTween?.Kill();

        Vector3 baseSize = new Vector3(m_defaultCursorSize, m_defaultCursorSize, 5f);
        Vector3 punchSize = new Vector3(baseSize.x * 1.2f, baseSize.y * 1.2f, baseSize.z);
        m_cursorWidget.size = baseSize;

        Sequence sizeSequence = DOTween.Sequence();
        sizeSequence.Append(
            DOTween.To(() => m_cursorWidget.size, x => m_cursorWidget.size = x, punchSize, 0.05f)
                .SetEase(Ease.OutBack)
        );
        sizeSequence.Join(
            DOTween.To(() => GetHighlightShaderValue(), SetHighlightShaderValue, 0, 0.05f)
                .SetEase(Ease.OutBack)
        );
        sizeSequence.Append(
            DOTween.To(() => m_cursorWidget.size, x => m_cursorWidget.size = x, baseSize, 0.1f)
                .SetEase(Ease.InOutSine)
        );
        sizeSequence.Join(
            DOTween.To(() => GetHighlightShaderValue(), SetHighlightShaderValue, .7f , 0.1f)
                .SetEase(Ease.InOutSine)
        );

        m_cursorSizeTween = sizeSequence;
        m_highlightShaderTween = sizeSequence;
        yield return sizeSequence.WaitForCompletion();
    }

    private IEnumerator UnhighlightCursor()
    {
        this.Log("Unhighlighting cursor");
        m_isCursorHighlighted = false;
        m_cursorSizeTween?.Kill();
        m_highlightShaderTween?.Kill();

        Vector3 targetSize = new Vector3(m_defaultCursorSize, m_defaultCursorSize, 5f);
        Sequence unhighlightSequence = DOTween.Sequence();
        unhighlightSequence.Append(
            DOTween.To(() => m_cursorWidget.size, x => m_cursorWidget.size = x, targetSize, 0.1f)
                .SetEase(Ease.InOutSine)
        );
        unhighlightSequence.Join(
            DOTween.To(() => GetHighlightShaderValue(), SetHighlightShaderValue, 2, 0.1f)
                .SetEase(Ease.InOutSine)
        );

        m_cursorSizeTween = unhighlightSequence;
        m_highlightShaderTween = unhighlightSequence;
        yield return unhighlightSequence.WaitForCompletion();
    }

    private float GetHighlightShaderValue()
    {
        Material decalMaterial = m_cursorWidget != null ? m_cursorWidget.material : null;
        if (decalMaterial == null || decalMaterial.HasProperty(m_highlightShaderProperty) == false)
        {
            return 0f;
        }

        return decalMaterial.GetFloat(m_highlightShaderProperty);
    }

    private void SetHighlightShaderValue(float value)
    {
        Material decalMaterial = m_cursorWidget != null ? m_cursorWidget.material : null;
        if (decalMaterial == null || decalMaterial.HasProperty(m_highlightShaderProperty) == false)
        {
            return;
        }

        decalMaterial.SetFloat(m_highlightShaderProperty, value);
    }
}