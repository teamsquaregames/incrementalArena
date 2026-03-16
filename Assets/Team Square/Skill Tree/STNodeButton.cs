using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using UnityEngine.UI;
using GIGA.AutoRadialLayout;
using UnityEngine.EventSystems;
using MPUIKIT;
using System.Collections;
using DG.Tweening;
using MyBox;
using Stats;


public class STNodeButton : CustomButton
{
    #region Fields
    [TitleGroup("Dependencies")]
    [SerializeField, Required] private STNodeAsset m_asset;
    [TitleGroup("Dependencies")]
    [SerializeField, Required] private RadialLayoutNode m_radialLayoutNode;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private Image m_icon;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private Image m_background;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private MPImage[] m_frames;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private Image m_sheen;
    [TitleGroup("Dependencies - Display")]
    [SerializeField] private GameObject m_subLayoutBG;
    [TitleGroup("Dependencies - Display")]
    [SerializeField] private SerializableDictionary<NodeRank, GameObject> m_ranksVisuals;
    [TitleGroup("Dependencies - Display")]
    [SerializeField] private GameObject m_levelsParent;
    [TitleGroup("Dependencies - Display")]
    [SerializeField] private GameObject m_affordableParent;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private GameObject[] m_levelObjects;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private GameObject[] m_levelActivatedObjects;
    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required] private CanvasGroup m_contentCanvasGroup;

    [TitleGroup("Dependencies - Display")]
    [SerializeField, Required, Space] private GameObject m_demoLockObject;

    [TitleGroup("Dependencies - Max Level Flash")]
    [SerializeField, Required] private Image m_maxLevelFlashImage;

    [TitleGroup("Settings")]
    [SerializeField] private bool m_lockedByDefault = true;
    [TitleGroup("Settings")]
    [SerializeField] private bool m_demoLocked = false;

    [TitleGroup("Settings")]
    [SerializeField] private float m_level0Opacity = .3f;
    [TitleGroup("Settings")]
    [SerializeField] private float m_lerpDuration = 0.5f;
    [TitleGroup("Settings")]
    [SerializeField] private Color m_activeBackgroundColor = Color.white;
    [TitleGroup("Settings")]
    [SerializeField] private Color m_defaultSheenColor = Color.white;
    [TitleGroup("Settings")]
    [SerializeField] private Color m_hoverSheenColor = Color.red;
    
    private RadialLayoutLink m_arrivingLink;
    private int m_level = 0;
    private Coroutine m_colorLerpCoroutine;
    private bool m_isLockInitialized = false;
    private PanelController m_panelController;
    
    private float m_maxLevelFlashDuration = 0.6f;
    private float m_maxLevelFlashStartScale = 1.5f;
    private float m_maxLevelFlashEndScale = 0.5f;
    private Ease m_maxLevelFlashScaleEase = Ease.OutQuad;
    private Ease m_maxLevelFlashFadeEase = Ease.InQuad;
    
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
        m_icon.sprite = m_asset.Icon;
        m_radialLayoutNode.onSetArrivingLink += onSetArrivingLink;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (m_maxLevelFlashImage != null)
            m_maxLevelFlashImage.gameObject.SetActive(false);

        m_level = GameData.Instance.GetNodeLevel(m_asset.ID);
        
        for (int i = 4; i >= m_asset.MaxLevel; i--)
            m_levelObjects[i].SetActive(false);

        if (m_level > 0)
        {
            m_demoLockObject.SetActive(m_demoLocked && GameConfig.Instance.gameSettings.isDemo);
            
            ApplyStatModifiers();
            SetChildrenLock(false);
            SetLock(false);
            SetActivatedNodeFeedback(true);
        }
        else
        {
            m_sheen.gameObject.SetActive(false);
            m_demoLockObject.SetActive(m_demoLocked && GameConfig.Instance.gameSettings.isDemo);

            if (m_subLayoutBG != null)
                m_subLayoutBG.SetActive(false);
            
            if (!m_isLockInitialized)
                SetLock(m_lockedByDefault);

            SetChildrenLock(true);
            m_levelsParent.SetActive(false);
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
        
        UIManager.Instance.GetCanvas<SkillTreeCanvas>().GetContainer<STNodeDetailsUIC>().Setup(this);
        
        if (m_colorLerpCoroutine != null)
            StopCoroutine(m_colorLerpCoroutine);
        m_colorLerpCoroutine = StartCoroutine(LerpColor(m_sheen, m_hoverSheenColor, m_lerpDuration));
        
        MouseCursorSetter.Instance.SetCursorHighlight(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!m_button.interactable) return;

        base.OnPointerExit(eventData);
        
        UIManager.Instance.GetCanvas<SkillTreeCanvas>().GetContainer<STNodeDetailsUIC>().Close();

        if (m_colorLerpCoroutine != null)
            StopCoroutine(m_colorLerpCoroutine);
        m_colorLerpCoroutine = StartCoroutine(LerpColor(m_sheen, m_defaultSheenColor, m_lerpDuration));
        
        MouseCursorSetter.Instance.SetCursorHighlight(false);
    }

    public override void SetHighlighted(bool _highlighted)
    {
        base.SetHighlighted(_highlighted);

        m_content.gameObject.SetActive(true);
        m_background.gameObject.SetActive(!m_isLocked);

        foreach (Image frame in m_frames)
            frame.gameObject.SetActive(!m_isLocked);

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
        
        if (GameData.Instance.GetNodeLevel(m_asset.ID) < m_asset.MaxLevel - 1)
            SoundManager.Instance.PlaySound(SoundKeys.ui_TTnode_click, Mathf.Lerp(.8f, 1.25f, (float)GameData.Instance.GetNodeLevel(m_asset.ID) / m_asset.MaxLevel));

        if (GameData.Instance.GetNodeLevel(m_asset.ID) == m_asset.MaxLevel - 1)
            SoundManager.Instance.PlaySound(SoundKeys.ui_TTnode_maxlevel);
    }

    private void TryLevelUpNode()
    {
        if (m_demoLocked && GameConfig.Instance.gameSettings.isDemo)
        {
            RejectClick();
            return;
        }

        if (GameData.Instance.GetNodeLevel(m_asset.ID) >= m_asset.MaxLevel)
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

    private void ApplyStatModifiers()
    {
        foreach (LeveledStatModifier statModifier in m_asset.StatModifiers)
            StatManager.Instance.AddDefinitionModifier(statModifier.entityType, statModifier.GetModifierAtLevel(m_level - 1));
    }

    private void SetChildrenLock(bool locked)
    {
        foreach (RadialLayoutNode child in m_radialLayoutNode.GetChildNodes())
            child.GetComponent<STNodeButton>().SetLock(locked);
    }

    private void LevelUpNode()
    {
        ClickBounce();
        m_level = GameData.Instance.LevelUpNode(m_asset.ID);
        
        ApplyStatModifiers();
        
        if (m_level == m_asset.MaxLevel)
            PlayMaxLevelFlashEffect();

        if (m_level == 1)
        {
            SetActivatedNodeFeedback(true);
            SetChildrenLock(false);
        }

        for (int i = 0; i < m_levelActivatedObjects.Length; i++)
            m_levelActivatedObjects[i].SetActive(i < m_level);

        GameData.Instance.IncrementTrackedValue(TrackedValueType.NodeUpgradesPurchased, 1);
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
        m_background.color = m_activeBackgroundColor.WithAlphaSetTo(activated ? 1 : 0.75f);
        
        foreach (MPImage frame in m_frames)
            frame.color = frame.color.WithAlphaSetTo(activated ? 1 : 0.1f);
        
        m_sheen.gameObject.SetActive(activated);

        if (m_arrivingLink != null && activated)
            m_arrivingLink.ProgressValue = 1;

        if (m_subLayoutBG != null && m_radialLayoutNode.IsSubLayout)
            m_subLayoutBG.SetActive(activated);

        m_levelsParent.SetActive(activated);

        for (int i = 0; i < m_levelActivatedObjects.Length; i++)
            m_levelActivatedObjects[i].SetActive(i < m_level);
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

        if (m_asset.MaxLevel <= m_level)
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
        if (m_icon != null && m_asset != null)
            m_icon.sprite = m_asset.Icon;

        if (m_ranksVisuals != null)
        {
            foreach (var kvp in m_ranksVisuals)
                kvp.Value.SetActive(false);

            if (m_asset != null && m_ranksVisuals.ContainsKey(m_asset.Rank))
                m_ranksVisuals[m_asset.Rank].SetActive(true);
        }

        m_previousAsset = m_asset;
    }
#endif
}