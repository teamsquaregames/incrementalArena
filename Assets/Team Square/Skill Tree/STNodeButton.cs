using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using UnityEngine.UI;
using GIGA.AutoRadialLayout;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;
using MyBox;
using Stats;
using TMPro;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

[Serializable]
public class NodeRankVisual
{
    public Sprite m_frameSprite;
    public Sprite m_backgroundSprite;
    public Sprite m_sigilSprite;
    public float m_scale = 1f;
}


public class STNodeButton : CustomButton
{
    #region Fields
    [SerializeField, Required, TitleGroup("Node Asset", order: -1)] private STNodeAsset m_asset;

    [SerializeField, Required, TitleGroup("Dependencies")] private RadialLayoutNode m_radialLayoutNode;
    [SerializeField, Required, TitleGroup("Dependencies")] private Image m_icon;
    [SerializeField, Required, TitleGroup("Dependencies")] private Image m_background;
    [SerializeField, Required, TitleGroup("Dependencies")] private Image m_frame;
    [SerializeField, Required, TitleGroup("Dependencies")] private Image m_sigil;
    [SerializeField, Required, TitleGroup("Dependencies")] private SerializableDictionary<NodeRank, NodeRankVisual> m_ranksVisuals;
    [SerializeField, Required, TitleGroup("Dependencies")] private GameObject m_levelsParent;
    [SerializeField, Required, TitleGroup("Dependencies")] private GameObject m_unlimitedLevelParent;
    [SerializeField, Required, TitleGroup("Dependencies")] private TMP_Text m_unlimitedLevelText;
    [SerializeField, Required, TitleGroup("Dependencies")] private GameObject m_affordableParent;
    [SerializeField, Required, TitleGroup("Dependencies")] private NodeLevel[] m_nodeLevels;
    [SerializeField, Required, TitleGroup("Dependencies")] private CanvasGroup m_contentCanvasGroup;
    [SerializeField, Required, TitleGroup("Dependencies")] private GameObject m_demoLockObject;
    [SerializeField, Required, TitleGroup("Dependencies")] private Image m_maxLevelFlashImage;

    private STNodeDetailsUIC m_detailsUI;


    [SerializeField, TitleGroup("Settings")] private bool m_lockedByDefault = true;
    [SerializeField, TitleGroup("Settings")] private bool m_demoLocked = false;

    [SerializeField, FoldoutGroup("Breathe")] private float m_breatheScale = 1.08f;
    [SerializeField, FoldoutGroup("Breathe")] private float m_breatheIconScale = 1.14f;
    [SerializeField, FoldoutGroup("Breathe")] private float m_breatheHalfDuration = 0.6f;
    [SerializeField, FoldoutGroup("Breathe")] private Ease m_breatheEase = Ease.InOutSine;

    [SerializeField, FoldoutGroup("Completion flash")] private float m_maxLevelFlashDuration = 0.6f;
    [SerializeField, FoldoutGroup("Completion flash")] private float m_maxLevelFlashStartScale = 1.5f;
    [SerializeField, FoldoutGroup("Completion flash")] private float m_maxLevelFlashEndScale = 0.5f;
    [SerializeField, FoldoutGroup("Completion flash")] private Ease m_maxLevelFlashScaleEase = Ease.OutQuad;
    [SerializeField, FoldoutGroup("Completion flash")] private Ease m_maxLevelFlashFadeEase = Ease.InQuad;

    private Sequence m_breatheSequence;
    private RadialLayoutLink m_arrivingLink;
    private int m_level = 0;
    private bool m_isLockInitialized = false;
    private PanelController m_panelController;


    public STNodeAsset LinkedNodeAsset => m_asset;
    public PanelController PanelController { get; set; }
    #endregion

    [Button]
    private void GetReferences()
    {
        m_button = GetComponent<Button>();
        m_radialLayoutNode = GetComponent<RadialLayoutNode>();
        m_contentCanvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    public override void Init()
    {
        // this.Log($"Initializing STNodeButton: {name}");
        m_radialLayoutNode.onSetArrivingLink += onSetArrivingLink;
        m_detailsUI = UIManager.Instance.GetCanvas<SkillTreeCanvas>().GetComponentInChildren<STNodeDetailsUIC>(true);
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (m_maxLevelFlashImage != null)
            m_maxLevelFlashImage.gameObject.SetActive(false);

        m_level = GameData.Instance.GetNodeLevel(m_asset.ID);

        if (!m_asset.IsUnlimited)
        {
            for (int i = m_asset.MaxLevel; i < m_nodeLevels.Length; i++)
                m_nodeLevels[i].gameObject.SetActive(false);
        }

        m_demoLockObject.SetActive(m_demoLocked && GameConfig.Instance.gameSettings.isDemo);

        if (m_level > 0)
        {
            ApplyStatModifiers();
            SetChildrenLock(false);
            SetLock(false);
            SetActivatedNodeFeedback(true);
        }
        else
        {
            if (!m_isLockInitialized)
                SetLock(m_lockedByDefault);

            SetChildrenLock(true);
            SetActivatedNodeFeedback(false);
        }
    }

    public override void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData.button != PointerEventData.InputButton.Left) return;
        if (!m_button.interactable) return;

        base.OnPointerClick(_eventData);
        TryLevelUpNode();
    }

    public override void SetLock(bool _isLocked)
    {
        m_isLockInitialized = true;
        base.SetLock(_isLocked);
        SetInteractible(!_isLocked);
        m_contentCanvasGroup.alpha = 1;

        m_content.gameObject.SetActive(!_isLocked);
        if (m_arrivingLink != null)
            m_arrivingLink.gameObject.SetActive(!_isLocked);

        if (!_isLocked)
        {
            GameData.Instance.onCurrencyChanged += OnCurrencyChange;
            OnCurrencyChange();
            m_background.gameObject.SetActive(true);
            m_frame.gameObject.SetActive(true);
            m_sigil.gameObject.SetActive(true);
        }
        else
        {
            GameData.Instance.onCurrencyChanged -= OnCurrencyChange;
            m_affordableParent.SetActive(false);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!m_button.interactable) return;
        if (eventData.pointerCurrentRaycast.gameObject != m_content.gameObject) return;

        base.OnPointerEnter(eventData);

        m_detailsUI.Setup(this);

        MouseCursorSetter.Instance?.SetCursorHighlight(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!m_button.interactable) return;

        base.OnPointerExit(eventData);

        m_detailsUI.Close();

        MouseCursorSetter.Instance?.SetCursorHighlight(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        StopBreatheTween();
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!m_button.interactable || m_isLocked || !m_isPressed) return;

        m_isPressed = false;

        if (m_buttonSettings.bounceOnClick)
        {
            // Bounce ends at Vector3.one so breathe always starts clean from baseline
            m_tweenSequence = DOTween.Sequence().SetUpdate(true);
            m_tweenSequence.Append(m_content.DOScale(Vector3.one * m_buttonSettings.clickBounceScale, m_buttonSettings.clickScaleDuration).SetEase(Ease.OutQuad));
            m_tweenSequence.Append(m_content.DOScale(Vector3.one, m_buttonSettings.clickScaleDuration).SetEase(Ease.OutQuad));
            m_tweenSequence.OnComplete(() => { if (m_isHovered) StartBreatheTween(); });
        }
        else
        {
            if (m_isHovered) StartBreatheTween();
            else m_hoverTween = m_content.DOScale(Vector3.one, m_buttonSettings.hoverExitDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    protected override void ScaleOnHoverEnter()
    {
        m_hoverTween?.Kill();
        StartBreatheTween();
    }

    protected override void ScaleOnHoverExit()
    {
        StopBreatheTween();
        m_hoverTween?.Kill();
        m_hoverTween = m_content.DOScale(Vector3.one, m_buttonSettings.hoverExitDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
        m_icon.transform.DOScale(Vector3.one, m_buttonSettings.hoverExitDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
    }

    protected override void ResetHoverScale()
    {
        StopBreatheTween();
        m_icon.transform.localScale = Vector3.one;
        base.ResetHoverScale();
    }

    private void StartBreatheTween()
    {
        m_breatheSequence?.Kill();
        m_breatheSequence = DOTween.Sequence().SetUpdate(true);
        m_breatheSequence.Append(m_content.DOScale(Vector3.one * m_breatheScale, m_breatheHalfDuration).SetEase(m_breatheEase));
        m_breatheSequence.Join(m_icon.transform.DOScale(Vector3.one * m_breatheIconScale, m_breatheHalfDuration).SetEase(m_breatheEase));
        m_breatheSequence.Append(m_content.DOScale(Vector3.one, m_breatheHalfDuration).SetEase(m_breatheEase));
        m_breatheSequence.Join(m_icon.transform.DOScale(Vector3.one, m_breatheHalfDuration).SetEase(m_breatheEase));
        m_breatheSequence.SetLoops(-1);
    }

    private void StopBreatheTween()
    {
        m_breatheSequence?.Kill();
        m_breatheSequence = null;
        m_icon.transform.localScale = Vector3.one;
    }

    public override void SetHighlighted(bool _highlighted)
    {
        base.SetHighlighted(_highlighted);

        m_content.gameObject.SetActive(true);
        m_background.gameObject.SetActive(!m_isLocked);

        SetInteractible(!m_isLocked);
        m_contentCanvasGroup.alpha = m_isLocked && !_highlighted ? 0 : 1;
    }

    private void onSetArrivingLink(RadialLayoutLink _link)
    {
        m_arrivingLink = _link;
        if (m_arrivingLink != null)
        {
            m_arrivingLink.gameObject.SetActive(!m_isLocked);
            m_radialLayoutNode.onSetArrivingLink -= onSetArrivingLink;
        }
        else
        {
            this.Log($"Arriving link is null.");
        }
    }

    public override void PlayClickSound()
    {
        if (!CanAfford()) return;

        if (m_asset.IsUnlimited)
        {
            SoundManager.Instance?.PlaySound(SoundKeys.ui_TTnode_click, 1f);
            return;
        }

        if (GameData.Instance.GetNodeLevel(m_asset.ID) < m_asset.MaxLevel - 1)
            SoundManager.Instance?.PlaySound(SoundKeys.ui_TTnode_click, Mathf.Lerp(.8f, 1.25f, (float)GameData.Instance.GetNodeLevel(m_asset.ID) / m_asset.MaxLevel));

        if (GameData.Instance.GetNodeLevel(m_asset.ID) == m_asset.MaxLevel - 1)
            SoundManager.Instance?.PlaySound(SoundKeys.ui_TTnode_maxlevel);
    }

    private void TryLevelUpNode()
    {
        if (m_demoLocked && GameConfig.Instance.gameSettings.isDemo)
        {
            RejectClick();
            return;
        }

        if (!m_asset.IsUnlimited && GameData.Instance.GetNodeLevel(m_asset.ID) >= m_asset.MaxLevel)
        {
            RejectClick();
            return;
        }

        if (CanAfford())
        {
            LevelUpNode();
            SpendCurrencies(m_level - 1);
        }
        else
        {
            RejectClick();
        }
    }

    private bool CanAfford() => CheckAffordability(true);

    private bool CheckAffordability(bool spendCheck)
    {
        foreach (Cost cost in m_asset.Cost)
        {
            if (!GameData.Instance.HasEnoughCurrency(cost.currencyAsset, cost.GetAmount(m_level), spendCheck))
                return false;
        }
        return true;
    }

    private void SpendCurrencies(int level)
    {
        foreach (Cost cost in m_asset.Cost)
            GameData.Instance.SpendCurrency(cost.currencyAsset, cost.GetAmount(level));
    }

    private void RejectClick()
    {
        NegativeClickBounce();
        PlayNegativeClickSound();
    }

    private void SetChildrenLock(bool locked)
    {
        foreach (RadialLayoutNode child in m_radialLayoutNode.GetChildNodes())
            child.GetComponent<STNodeButton>().SetLock(locked);
    }

    private void LevelUpNode()
    {
        ClickBounce();

        if (GameData.Instance == null) return;

        m_level = GameData.Instance.LevelUpNode(m_asset.ID);

        UnlockAbility();
        UnlockEnemy();
        UnlockArena();
        ApplyStatModifiers();

        if (m_level == 1)
        {
            SetActivatedNodeFeedback(true);
            SetChildrenLock(false);
        }

        if (m_asset.IsUnlimited)
        {
            if (m_unlimitedLevelText != null) m_unlimitedLevelText.text = m_level.ToString();
        }
        else
        {
            if (m_level == m_asset.MaxLevel)
                PlayMaxLevelFlashEffect();

            for (int i = 0; i < m_nodeLevels.Length; i++)
            {
                if (i == m_level - 1)
                    m_nodeLevels[i].Activate();
                else
                    m_nodeLevels[i].SetActive(i < m_level);
            }
        }

        GameData.Instance.IncrementTrackedValue(TrackedValueType.NodeUpgradesPurchased, 1);
    }

    private void UnlockAbility()
    {
        if (m_asset.AbilitiesGranted != null)
        {
            foreach (var ability in m_asset.AbilitiesGranted)
            {
                if (ability != null)
                    GameData.Instance.UnlockAbility(ability);
            }
        }
    }

    private void UnlockEnemy()
    {
        if (m_asset.EnemiesUnlocked != null)
        {
            foreach (var enemy in m_asset.EnemiesUnlocked)
            {
                if (enemy != null)
                    GameData.Instance.UnlockEnemy(enemy);
            }
        }
    }

    private void UnlockArena()
    {
        if (m_asset.ArenaUnlocked != null)
            GameData.Instance.UnlockArena(m_asset.ArenaUnlocked.arenaID);
    }

    private void ApplyStatModifiers()
    {
        if (StatManager.Instance == null) return;

        foreach (LeveledStatModifier statModifier in m_asset.StatModifiers)
        {
            var modifier = m_asset.IsUnlimited
                ? statModifier.GetLinearModifierAtLevel(m_level)
                : statModifier.GetModifierAtLevel(m_level - 1);

            if (statModifier.entityType != 0)
                StatManager.Instance.AddDefinitionModifier(statModifier.entityType, modifier);

            if (statModifier.ability != null)
                StatManager.Instance.AddDefinitionModifier(statModifier.ability, modifier, statModifier.step, statModifier.application);
        }
    }


    private void PlayMaxLevelFlashEffect()
    {
        if (m_maxLevelFlashImage == null)
        {
            Debug.LogWarning($"Max level flash image not assigned for node: {m_asset.DisplayName}");
            return;
        }

        m_maxLevelFlashImage.DOKill();
        m_maxLevelFlashImage.transform.DOKill();

        m_maxLevelFlashImage.gameObject.SetActive(true);
        m_maxLevelFlashImage.transform.localScale = Vector3.one * m_maxLevelFlashStartScale;
        m_maxLevelFlashImage.color = new Color(
            m_maxLevelFlashImage.color.r,
            m_maxLevelFlashImage.color.g,
            m_maxLevelFlashImage.color.b,
            1f
        );

        Sequence flashSequence = DOTween.Sequence();
        flashSequence.Join(m_maxLevelFlashImage.transform.DOScale(m_maxLevelFlashEndScale, m_maxLevelFlashDuration).SetEase(m_maxLevelFlashScaleEase));
        flashSequence.Join(m_maxLevelFlashImage.DOFade(0f, m_maxLevelFlashDuration).SetEase(m_maxLevelFlashFadeEase));
        flashSequence.OnComplete(() => m_maxLevelFlashImage.gameObject.SetActive(false));
        flashSequence.Play();
    }

    private void SetActivatedNodeFeedback(bool activated)
    {
        if (m_arrivingLink != null && activated)
            m_arrivingLink.ProgressValue = 1;

        if (m_asset.IsUnlimited)
        {
            if (m_unlimitedLevelParent != null) m_unlimitedLevelParent.SetActive(activated);
            if (activated && m_unlimitedLevelText != null) m_unlimitedLevelText.text = m_level.ToString();
        }
        else
        {
            m_levelsParent.SetActive(activated);
            for (int i = 0; i < m_nodeLevels.Length; i++)
                m_nodeLevels[i].SetActive(i < m_level);
        }
    }

    private IEnumerator LerpColor(Image _image, Color _targetColor, float _duration)
    {
        Color initialColor = _image.color;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            _image.color = Color.Lerp(initialColor, _targetColor, elapsed / _duration);
            yield return null;
        }

        _image.color = _targetColor;
    }

    private void OnCurrencyChange(CurrencyAsset _currencyAsset, double _newAmount) => OnCurrencyChange();

    private void OnCurrencyChange()
    {
        if (m_affordableParent == null) return;

        if (!m_asset.IsUnlimited && m_asset.MaxLevel <= m_level)
        {
            m_affordableParent.SetActive(false);
            return;
        }

        m_affordableParent.SetActive(CheckAffordability(false));
    }

#if UNITY_EDITOR
    private STNodeAsset m_previousAsset;

    private void OnValidate()
    {
        if (m_asset != m_previousAsset)
            OnAssetChanged();
    }

    [Button]
    private void OnAssetChanged()
    {
        m_previousAsset = m_asset;

        if (m_asset == null) return;

        name = $"{m_asset.ID}";

        // Icon
        if (m_icon != null)
            m_icon.sprite = m_asset.Icon;

        // Rank visuals
        if (m_ranksVisuals != null && m_ranksVisuals.TryGetValue(m_asset.Rank, out NodeRankVisual visual))
        {
            if (m_frame != null) m_frame.sprite = visual.m_frameSprite;
            if (m_background != null) m_background.sprite = visual.m_backgroundSprite;
            if (m_sigil != null) m_sigil.sprite = visual.m_sigilSprite;
            if (transform is RectTransform rectTransform)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, visual.m_scale * 100);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, visual.m_scale * 100);
            }
        }

        // Level visuals — show pips for Limited, text for Unlimited
        bool isUnlimited = m_asset.IsUnlimited;
        if (m_levelsParent != null) m_levelsParent.SetActive(!isUnlimited);
        if (m_unlimitedLevelParent != null) m_unlimitedLevelParent.SetActive(isUnlimited);
        if (isUnlimited && m_unlimitedLevelText != null) m_unlimitedLevelText.text = "0";
        if (!isUnlimited && m_nodeLevels != null)
        {
            for (int i = 0; i < m_nodeLevels.Length; i++)
                m_nodeLevels[i].gameObject.SetActive(i < m_asset.MaxLevel);
        }

        // Demo lock — preview based on the flag alone (no GameConfig in editor)
        if (m_demoLockObject != null)
            m_demoLockObject.SetActive(m_demoLocked);

        // Affordable — always off in editor, no currency context
        if (m_affordableParent != null)
            m_affordableParent.SetActive(false);
    }
#endif
}