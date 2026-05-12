using System;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomButton : AUIElement, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    #region Inner Class
    [System.Serializable]
    public class ButtonSettings
    {
        [FoldoutGroup("Click")] public bool bounceOnClick = true;
        [FoldoutGroup("Click")] public float clickScaleDuration = 0.07f;
        [FoldoutGroup("Click")] public float clickBounceScale = 1.15f;
        [FoldoutGroup("Click")] public float pressedScale = 0.9f;

        [FoldoutGroup("Hover")] public float hoverEnterDuration = 0.07f;
        [FoldoutGroup("Hover")] public float hoverExitDuration = 0.2f;
        [FoldoutGroup("Hover")] public float hoverScale = 1.1f;
        [FoldoutGroup("Hover")] public bool useHoverTextColor = false;
        [FoldoutGroup("Hover"), ShowIf("useHoverTextColor")]public Color hoverColor = Color.white;

        [FoldoutGroup("Locked")] public float lockedShakeDuration = 0.3f;
        [FoldoutGroup("Locked")] public float lockedShakeStrength = 30f;
        [FoldoutGroup("Locked")] public int lockedShakeVibrato = 20;
    }
    #endregion

    #region Actions
    [TitleGroup("Actions")]
    public Action<int> onClick;
    public Action<int> onHoverEnter;
    public Action<int> onHoverExit;
    #endregion

    #region Dependencies
    [FoldoutGroup("Dependencies"), Required]
    [SerializeField] protected Button m_button;
    [FoldoutGroup("Dependencies")]
    [SerializeField] protected RectTransform m_lockedContent;
    [FoldoutGroup("Dependencies")]
    [SerializeField] private GameObject m_highlightObject;
    #endregion

    #region Settings
    
    [TitleGroup("Settings")]
    [SerializeField] protected ButtonSettings m_buttonSettings = new ButtonSettings();

    #endregion

    #region Var
    protected Sequence m_tweenSequence = null;
    protected Tween m_hoverTween = null;
    protected bool m_isLocked;
    protected bool m_isHovered;
    protected bool m_isPressed;
    protected int m_index;
    private TMPro.TMP_Text[] m_texts;
    private Image[] m_images;
    private Color[] m_originalTextColors;
    private Color[] m_originalImageColors;
    #endregion
    
    public int Index => m_index;

    [Button]
    public void CacheReferences()
    {
        Transform contentTfm = transform.Find("Content");
        if (contentTfm != null && contentTfm.TryGetComponent(out RectTransform rectTransform))
        {
            m_content = rectTransform;
        }

        m_button = GetComponent<Button>();
    }

    public override void Init()
    {
        CacheHoverColorTargets();
    }

    public virtual void Init(int _index)
    {
        SetIndex(_index);
        CacheHoverColorTargets();
    }

    private void CacheHoverColorTargets()
    {
        if (m_content == null) return;

        m_texts = m_content.GetComponentsInChildren<TMPro.TMP_Text>(true);
        m_originalTextColors = new Color[m_texts.Length];
        for (int i = 0; i < m_texts.Length; i++)
            m_originalTextColors[i] = m_texts[i].color;

        m_images = m_content.GetComponentsInChildren<Image>(true);
        m_originalImageColors = new Color[m_images.Length];
        for (int i = 0; i < m_images.Length; i++)
            m_originalImageColors[i] = m_images[i].color;

    }

    public virtual void SetLock(bool _isLocked)
    {
        m_isLocked = _isLocked;

        if (m_lockedContent != null)
            m_lockedContent.gameObject.SetActive(_isLocked);
    }

    public virtual void SetInteractible(bool _isInteractible)
    {
        m_button.interactable = _isInteractible;

        if (!_isInteractible && m_isHovered)
        {
            m_isHovered = false;
            m_isPressed = false;
            ResetHoverScale();
            ResetTextColor();
        }
    }

    public virtual void SetHighlighted(bool _highlighted)
    {
        if (m_highlightObject != null)
            m_highlightObject.gameObject.SetActive(_highlighted);
    }

    [Button("Highlight (Debug)")]
    private void DebugHighlight() => SetHighlighted(true);

    [Button("Unhighlight (Debug)")]
    private void DebugUnhighlight() => SetHighlighted(false);

    public virtual void UpdateValues() { }

    public virtual void Lock() { }

    public virtual void Unlock() { }

    [Button("Lock (Debug)")]
    private void DebugLock() => SetLock(true);

    [Button("Unlock (Debug)")]
    private void DebugUnlock() => SetLock(false);

    public virtual void PlayClickSound()
    {
        SoundManager.Instance?.PlaySound(SoundKeys.ui_button_click_negative);
    }

    public virtual void PlayNegativeClickSound()
    {
        SoundManager.Instance?.PlaySound(SoundKeys.ui_button_click_negative);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!m_button.interactable || m_isLocked) return;

        m_isPressed = true;

        m_hoverTween?.Complete();
        m_hoverTween = m_content.DOScale(Vector3.one * m_buttonSettings.pressedScale, m_buttonSettings.hoverEnterDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!m_button.interactable || m_isLocked || !m_isPressed) return;

        m_isPressed = false;

        if (m_buttonSettings.bounceOnClick)
        {
            ClickBounce();
        }
        else
        {
            Vector3 targetScale = m_isHovered ? Vector3.one * m_buttonSettings.hoverScale : Vector3.one;

            m_hoverTween?.Complete();
            m_hoverTween = m_content.DOScale(targetScale, m_buttonSettings.hoverExitDuration)
                .SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!m_button.interactable) return;

        if (m_isLocked)
        {
            LockedClickBounce();
            PlayNegativeClickSound();
            return;
        }

        onClick?.Invoke(m_index);
        PlayClickSound();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!m_button.interactable || m_isLocked) return;

        m_isHovered = true;
        ScaleOnHoverEnter();
        SetHoverTextColor();
        onHoverEnter?.Invoke(m_index);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!m_button.interactable || m_isLocked) return;

        m_isHovered = false;
        m_isPressed = false;
        ScaleOnHoverExit();
        ResetTextColor();
        onHoverExit?.Invoke(m_index);
    }

    protected virtual void ScaleOnHoverEnter()
    {
        m_hoverTween?.Complete();
        m_hoverTween = m_content.DOScale(Vector3.one * m_buttonSettings.hoverScale, m_buttonSettings.hoverEnterDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
    }

    protected virtual void ScaleOnHoverExit()
    {
        m_hoverTween?.Complete();
        m_hoverTween = m_content.DOScale(Vector3.one, m_buttonSettings.hoverExitDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
    }

    protected virtual void ResetHoverScale()
    {
        m_hoverTween?.Kill();

        m_content.localScale = Vector3.one;

        KillColorTweens();
        for (int i = 0; i < m_texts.Length; i++)
            m_texts[i].color = m_originalTextColors[i];
        for (int i = 0; i < m_images.Length; i++)
            m_images[i].color = m_originalImageColors[i];
    }

    private void KillColorTweens()
    {
        for (int i = 0; i < m_texts.Length; i++)
            DOTween.Kill(m_texts[i]);
        for (int i = 0; i < m_images.Length; i++)
            DOTween.Kill(m_images[i]);
    }

    private void SetHoverTextColor()
    {
        if (m_buttonSettings.useHoverTextColor)
        {
            KillColorTweens();
            for (int i = 0; i < m_texts.Length; i++)
                m_texts[i].DOColor(m_buttonSettings.hoverColor, m_buttonSettings.hoverEnterDuration).SetUpdate(true);
            for (int i = 0; i < m_images.Length; i++)
                m_images[i].DOColor(m_buttonSettings.hoverColor, m_buttonSettings.hoverEnterDuration).SetUpdate(true);
        }

    }

    private void ResetTextColor()
    {
        if (m_buttonSettings.useHoverTextColor)
        {
            KillColorTweens();
            for (int i = 0; i < m_texts.Length; i++)
                m_texts[i].DOColor(m_originalTextColors[i], m_buttonSettings.hoverExitDuration).SetUpdate(true);
            for (int i = 0; i < m_images.Length; i++)
                m_images[i].DOColor(m_originalImageColors[i], m_buttonSettings.hoverExitDuration).SetUpdate(true);
        }

    }

    public void ClickBounce()
    {
        if (m_tweenSequence != null && m_tweenSequence.IsActive() && m_tweenSequence.IsPlaying()) return;

        Vector3 targetScale = m_isHovered ? Vector3.one * m_buttonSettings.hoverScale : Vector3.one;

        m_tweenSequence = DOTween.Sequence().SetUpdate(true);
        m_tweenSequence.Append(m_content.DOScale(Vector3.one * m_buttonSettings.clickBounceScale, m_buttonSettings.clickScaleDuration).SetEase(Ease.OutQuad));
        m_tweenSequence.Append(m_content.DOScale(targetScale, m_buttonSettings.clickScaleDuration).SetEase(Ease.OutQuad));
    }

    public void LockedClickBounce()
    {
        if (m_tweenSequence != null && m_tweenSequence.IsPlaying()) return;
        m_tweenSequence = DOTween.Sequence().SetUpdate(true);
        m_tweenSequence.Append(m_lockedContent.DOShakeAnchorPos(m_buttonSettings.lockedShakeDuration, m_buttonSettings.lockedShakeStrength, m_buttonSettings.lockedShakeVibrato, 90, false, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.OutQuad));
    }

    public void NegativeClickBounce()
    {
        if (m_tweenSequence != null && m_tweenSequence.IsActive() && m_tweenSequence.IsPlaying()) return;
        m_tweenSequence = DOTween.Sequence().SetUpdate(true);
        m_tweenSequence.Append(m_content.DOShakeAnchorPos(m_buttonSettings.lockedShakeDuration, m_buttonSettings.lockedShakeStrength, m_buttonSettings.lockedShakeVibrato, 90, false, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.OutQuad));
    }

    public void Hide(bool _isHidden)
    {
        //this.Log($"Hide {_isHidden}");
        m_content.gameObject.SetActive(!_isHidden);
        m_button.interactable = !_isHidden;

        if (_isHidden)
        {
            m_isHovered = false;
            m_isPressed = false;
            ResetHoverScale();
            ResetTextColor();
        }
    }

    public void SetIndex(int _index)
    {
        m_index = _index;
    }

    protected virtual void CheckButtonInteractible() { }
}