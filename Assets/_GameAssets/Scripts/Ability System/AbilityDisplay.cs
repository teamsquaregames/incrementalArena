using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class AbilityDisplay : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField, Required] private Image m_icon;
    [SerializeField, Required] private Image m_cooldownFill;
    [SerializeField, Required] private RectTransform m_iconRect;

    [TitleGroup("Feedback")]
    [SerializeField] private float m_punchScale = 0.3f;
    [SerializeField] private float m_punchDuration = 0.35f;
    [SerializeField] private float m_blinkAlpha = 0.2f;
    [SerializeField] private float m_blinkDuration = 0.08f;

    private AbilityConfig m_config;
    private Vector3 m_initialScale;

    public AbilityConfig Config => m_config;
    public bool IsAssigned => m_config != null;

    private void Awake()
    {
        m_initialScale = m_iconRect.localScale;
        m_cooldownFill.fillAmount = 0f;
    }

    public void Assign(AbilityConfig config)
    {
        m_config = config;
        m_icon.sprite = config.icon;
        m_cooldownFill.fillAmount = 0f;
        gameObject.SetActive(true);
    }

    public void Unassign()
    {
        m_iconRect.DOKill();
        m_icon.DOKill();
        m_config = null;
        m_cooldownFill.fillAmount = 0f;
        gameObject.SetActive(false);
    }

    public void PlayUsedFeedback()
    {
        m_iconRect.DOKill();
        m_iconRect.localScale = m_initialScale;
        m_iconRect.DOPunchScale(Vector3.one * m_punchScale, m_punchDuration, 8, 1f);

        m_icon.DOKill();
        m_icon.DOFade(m_blinkAlpha, m_blinkDuration)
            .OnComplete(() => m_icon.DOFade(1f, m_blinkDuration));
    }

    // fillAmount = 1 when just used, 0 when ready
    public void UpdateCooldown(float remaining, float total)
    {
        m_cooldownFill.fillAmount = total > 0f ? remaining / total : 0f;
    }
}
