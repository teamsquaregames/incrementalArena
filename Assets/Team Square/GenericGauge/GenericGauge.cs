using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;

public class GenericGauge : MonoBehaviour, IPoolable
{
    [Header("References")]
    [SerializeField] private Image fillImage;           // Filled image (Image Type: Filled)
    [SerializeField] private RectTransform fillAreaRect; // Parent rect used to position chunks
    [SerializeField] private Image flashOverlay;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Text Label")]
    [SerializeField] private bool showText = false;
    [SerializeField] private TMP_Text label;

    [Header("Visibility")]
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private float hideDelay = 1f;
    [SerializeField] private float hideFadeDuration = 0.3f;

    [Header("Timings")]
    [SerializeField] private float chunkPopDuration = 0.15f;
    [SerializeField] private float chunkExitDelay = 0.25f;
    [SerializeField] private float chunkExitDuration = 0.4f;
    [SerializeField] private float barShakeDuration = 0.15f;
    [SerializeField] private float smoothFillDuration = 0.3f;
    [SerializeField] private float chunkYScale = 2f;
    [SerializeField] private Color chunkColor = Color.white;

    [Header("Flash")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.white;

    [Header("Shake Settings")]
    [SerializeField] private float chunkShakeIntensity = 8f;
    [SerializeField] private float barShakeIntensity = 6f;

    [Header("Pooling")]
    [SerializeField] private Image damageChunkPrefab;
    [SerializeField] private int chunkPoolSize = 4;

    [Header("Spawn Animations")]
    [SerializeField] private float fadeInDuration = 0.2f;

    private List<Image> chunkPool;
    private List<Image> activeChunks = new List<Image>();
    private Transform target;

    private Tween sliderTween;
    private Tween barShakeTween;
    private Tween flashColorTween;
    private Tween fadeTween;
    private Tween hideDelayTween;
    private float currentNormalized;

    private void Awake()
    {
        currentNormalized = fillImage != null ? fillImage.fillAmount : 0f;
        InitializeChunkPool();

        if (label != null)
            label.gameObject.SetActive(showText);
    }

    private void Update()
    {
        if (target != null)
            transform.position = CameraManager.Instance.MainCam.WorldToScreenPoint(target.position);
    }

    public void Setup(Transform target, float currentValue, float maxValue)
    {
        this.target = target;
        SetValue(currentValue, maxValue, instant: true, showChunks: false);
    }

    public void SetValue(float currentValue, float maxValue)
    {
        SetValue(currentValue, maxValue, instant: false, showChunks: true);
    }

    public void SetValue(float currentValue, float maxValue, bool instant, bool showChunks)
    {
        maxValue = Mathf.Max(maxValue, 0.0001f);
        float targetNormalized = Mathf.Clamp01(currentValue / maxValue);

        if (targetNormalized >= 1 && hideWhenFull)
        {
            HideGauge(instant);
        }

        if (Mathf.Approximately(targetNormalized, currentNormalized))
            return;

        bool isDamage = targetNormalized < currentNormalized;
        float previousNormalized = currentNormalized;

        if (hideWhenFull && isDamage)
            ShowGauge();

        UpdateFill(targetNormalized, instant);
        UpdateLabel(currentValue, maxValue);

        if (showChunks)
        {
            SpawnChunk(previousNormalized, targetNormalized, isDamage);
            Flash();
            ShakeBar();
        }

        if (hideWhenFull && Mathf.Approximately(targetNormalized, 1f))
            HideGauge();
    }

    public void Reset()
    {
        sliderTween?.Kill();
        barShakeTween?.Kill();
        flashColorTween?.Kill();
        fadeTween?.Kill();
        hideDelayTween?.Kill();

        if (flashOverlay != null)
            flashOverlay.gameObject.SetActive(false);

        foreach (Image chunk in activeChunks.ToArray())
        {
            if (chunk != null)
            {
                DOTween.Kill(chunk.rectTransform);
                chunk.gameObject.SetActive(false);
            }
        }

        activeChunks.Clear();
    }

    #region Chunks

    private void InitializeChunkPool()
    {
        // Use fillAreaRect if assigned, otherwise fall back to fillImage's transform
        Transform poolParent = fillAreaRect != null ? fillAreaRect : fillImage.rectTransform;

        chunkPool = new List<Image>(chunkPoolSize);
        for (int i = 0; i < chunkPoolSize; i++)
        {
            Image chunk = Instantiate(damageChunkPrefab, poolParent);
            chunk.gameObject.SetActive(false);
            chunk.raycastTarget = false;
            chunkPool.Add(chunk);
        }
    }

    private Image GetChunkFromPool()
    {
        foreach (var chunk in chunkPool)
            if (!chunk.gameObject.activeSelf)
                return chunk;

        if (activeChunks.Count > 0)
        {
            Image reused = activeChunks[0];
            reused.gameObject.SetActive(false);
            activeChunks.RemoveAt(0);
            return reused;
        }

        Transform poolParent = fillAreaRect != null ? fillAreaRect : fillImage.rectTransform;
        Image extra = Instantiate(damageChunkPrefab, poolParent);
        extra.raycastTarget = false;
        chunkPool.Add(extra);
        return extra;
    }

    private void SpawnChunk(float from, float to, bool isDamage)
    {
        float left  = Mathf.Min(from, to);
        float right = Mathf.Max(from, to);

        if (right - left <= 0.0001f)
            return;

        Image chunk = GetChunkFromPool();
        SetupChunk(chunk, left, right, isDamage);
        chunk.gameObject.SetActive(true);
        activeChunks.Add(chunk);

        AnimateChunk(chunk);
    }

    private void SetupChunk(Image chunk, float left, float right, bool isDamage)
    {
        RectTransform rt = chunk.rectTransform;

        // Anchor the chunk across the fill area using normalized fill positions
        rt.anchorMin        = new Vector2(left,  0f);
        rt.anchorMax        = new Vector2(right, 1f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale       = Vector3.one;
        rt.pivot            = isDamage ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);

        chunk.color = chunkColor;
    }

    private void AnimateChunk(Image chunk)
    {
        RectTransform rt = chunk.rectTransform;
        DOTween.Kill(rt);

        rt.DOScaleY(chunkYScale, chunkPopDuration).SetEase(Ease.OutBack);
        rt.DOShakeAnchorPos(chunkPopDuration, chunkShakeIntensity, 25, 90, false, true);
        DOVirtual.DelayedCall(chunkExitDelay, () => PlayChunkExit(chunk));
    }

    private void PlayChunkExit(Image chunk)
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(chunk.DOFade(0f, chunkExitDuration));
        seq.SetEase(Ease.InCubic);
        seq.OnComplete(() => DeactivateChunk(chunk));
    }

    private void DeactivateChunk(Image chunk)
    {
        chunk.gameObject.SetActive(false);
        activeChunks.Remove(chunk);
    }

    #endregion

    #region IPoolable

    public void OnSpawn()
    {
        fadeTween?.Kill();
        canvasGroup.alpha = 0f;
        fadeTween = canvasGroup.DOFade(1f, fadeInDuration);
    }

    public void OnDespawn() { }

    #endregion

    // ------------------------------------------------------------------
    // Core fill update — replaces all slider.value usage
    // ------------------------------------------------------------------
    private void UpdateFill(float targetFill, bool instant)
    {
        sliderTween?.Kill();
        currentNormalized = targetFill;

        if (instant)
        {
            fillImage.fillAmount = targetFill;
        }
        else
        {
            float from = fillImage.fillAmount;
            sliderTween = DOTween.To(
                    () => from,
                    v => fillImage.fillAmount = v,
                    targetFill,
                    smoothFillDuration)
                .SetEase(Ease.OutCubic);
        }
    }

    private void UpdateLabel(float current, float max)
    {
        if (!showText || label == null) return;
        label.text = current.ToString("N0");
    }

    private void ShowGauge()
    {
        hideDelayTween?.Kill();
        fadeTween?.Kill();
        fadeTween = canvasGroup.DOFade(1f, hideFadeDuration);
    }

    public void HideGauge(bool _instant = false, TweenCallback onComplete = null)
    {
        hideDelayTween?.Kill();
        fadeTween?.Kill();

        if (_instant)
        {
            canvasGroup.alpha = 0;
            onComplete?.Invoke();
        }
        else
        {
            hideDelayTween = DOVirtual.DelayedCall(hideDelay, () =>
            {
                fadeTween?.Kill();
                fadeTween = canvasGroup.DOFade(0f, hideFadeDuration);

                if (onComplete != null)
                    fadeTween.onComplete += onComplete;
            });
        }
    }

    private void Flash()
    {
        flashColorTween?.Kill();

        flashOverlay.gameObject.SetActive(true);
        flashOverlay.color = flashColor;

        flashColorTween = flashOverlay.DOFade(0f, flashDuration);
        flashColorTween.onComplete += () => flashOverlay.gameObject.SetActive(false);
    }

    private void ShakeBar()
    {
        barShakeTween?.Complete();
        barShakeTween = transform.DOShakeRotation(
            barShakeDuration, Vector3.forward * barShakeIntensity, 5);
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
        flashColorTween?.Kill();
        barShakeTween?.Kill();
        sliderTween?.Kill();
        hideDelayTween?.Kill();
    }
}