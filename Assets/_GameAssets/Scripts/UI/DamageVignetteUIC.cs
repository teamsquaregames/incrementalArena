using System.Collections;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using Utils.UI;

public class DamageVignetteUIC : UIContainer
{
    [SerializeField] private CanvasGroup m_canvasGroup;
    [SerializeField] private float m_peakAlpha = 0.8f;
    [SerializeField] private float m_fadeInDuration = 0.05f;
    [SerializeField] private float m_fadeOutDuration = 0.4f;

    private Tween m_tween;
    private Coroutine m_lowHealthCoroutine;
    private float m_currentLowHealthAlpha;

    public void Flash(float damagePercentage)
    {
        m_tween?.Kill();
        m_canvasGroup.alpha = 0f;

        m_tween = m_canvasGroup.DOFade(math.clamp(damagePercentage, .2f, .5f) * 2 * m_peakAlpha, m_fadeInDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                m_tween = m_canvasGroup.DOFade(0f, m_fadeOutDuration)
                    .SetEase(Ease.InQuad);
            });
    }

    public void LowHealthWarning(float healthPercentage)
    {
        this.Log($"Low health warning. Health percentage: {healthPercentage}");
        m_currentLowHealthAlpha = CusMath.Remap(healthPercentage, 0.5f, 0f, 0f, m_peakAlpha);
        if (m_lowHealthCoroutine == null)
            m_lowHealthCoroutine = StartCoroutine(LowHealthWarningCR());
    }

    public void StopLowHealthWarning()
    {
        this.Log("Stopping low health warning.");
        if (m_lowHealthCoroutine != null)
        {
            StopCoroutine(m_lowHealthCoroutine);
            m_lowHealthCoroutine = null;
            m_currentLowHealthAlpha = 0f;
            m_canvasGroup.DOFade(0f, .5f).SetEase(Ease.OutQuad);
        }
    }

    private IEnumerator LowHealthWarningCR()
    {
        while (true)
        {
            m_canvasGroup.DOFade(m_currentLowHealthAlpha, .2f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(.2f);
            m_canvasGroup.DOFade(m_currentLowHealthAlpha - .1f, .2f).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(.2f);
        }
    }
}
