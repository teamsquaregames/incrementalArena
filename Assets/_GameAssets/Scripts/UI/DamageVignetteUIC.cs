using DG.Tweening;
using UnityEngine;
using Utils.UI;

public class DamageVignetteUIC : UIContainer
{
    [SerializeField] private CanvasGroup m_canvasGroup;
    [SerializeField] private float m_peakAlpha = 0.8f;
    [SerializeField] private float m_fadeInDuration = 0.05f;
    [SerializeField] private float m_fadeOutDuration = 0.4f;

    private Tween m_tween;

    public void Flash()
    {
        m_tween?.Kill();
        m_canvasGroup.alpha = 0f;

        m_tween = m_canvasGroup.DOFade(m_peakAlpha, m_fadeInDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                m_tween = m_canvasGroup.DOFade(0f, m_fadeOutDuration)
                    .SetEase(Ease.InQuad);
            });
    }
}
