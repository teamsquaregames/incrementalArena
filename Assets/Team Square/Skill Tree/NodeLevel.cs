using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NodeLevel : MonoBehaviour
{
    [SerializeField, Required] private Image m_activeImage;
    [SerializeField, Required] private Image m_flashImage;
    [SerializeField, Required] private CanvasGroup m_flashCanvasGroup;

    [TitleGroup("Flash Settings")]
    [SerializeField] private float m_flashDuration = 0.4f;
    [SerializeField] private float m_flashStartScale = 1.5f;
    [SerializeField] private float m_flashEndScale = 0.5f;
    [SerializeField] private Ease m_flashScaleEase = Ease.OutQuad;
    [SerializeField] private Ease m_flashFadeEase = Ease.InQuad;

    private void Awake()
    {
        m_flashImage.gameObject.SetActive(false);
        m_activeImage.gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        m_activeImage.gameObject.SetActive(active);
    }

    public void Activate()
    {
        m_activeImage.gameObject.SetActive(true);
        PlayFlash();
    }

    [Button]
    private void PlayFlash()
    {
        m_flashImage.transform.DOKill();
        DOTween.Kill(m_flashCanvasGroup);

        m_flashImage.gameObject.SetActive(true);
        m_flashImage.transform.localScale = Vector3.one * m_flashStartScale;
        m_flashCanvasGroup.alpha = 1f;

        Sequence seq = DOTween.Sequence();
        seq.Join(m_flashImage.transform.DOScale(m_flashEndScale, m_flashDuration).SetEase(m_flashScaleEase));
        seq.Join(m_flashCanvasGroup.DOFade(0f, m_flashDuration).SetEase(m_flashFadeEase));
        seq.OnComplete(() => m_flashImage.gameObject.SetActive(false));
        seq.Play();
    }
}
