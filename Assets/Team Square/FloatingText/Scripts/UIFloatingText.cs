using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using MyBox;
using Sirenix.OdinInspector;
using TMPro;

public class UIFloatingText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Transform m_parent;
    
    [SerializeField] private FloatingTextConfig m_linkedConfig;
    private Vector3 m_initialPosition;
    private Sequence _seq;
    
    [Button]
    public void Preview()
    {
        text.color = m_linkedConfig.color;
        text.font = m_linkedConfig.font;
        text.fontSize = m_linkedConfig.fontSize;

#if UNITY_EDITOR
        if (Application.isPlaying)
            Play();
#endif
    }

    private void Reset()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(string message, FloatingTextConfig config)
    {
        text.text = message;
        m_linkedConfig = config;
        text.color = m_linkedConfig.color;
        text.font = m_linkedConfig.font;
        text.fontSize = m_linkedConfig.fontSize;
    }

    public void Play()
    {
        text.color = text.color.WithAlphaSetTo(m_linkedConfig.enableFadeIn ? 0 : 1);
        m_parent.localScale = m_linkedConfig.enableScaleIn ? Vector3.zero : Vector3.one;
        m_parent.localPosition = Vector3.zero;

        if (_seq != null && _seq.IsPlaying())
            _seq.Kill();
        DOTween.Kill(text);
        DOTween.Kill(m_parent);

        _seq = DOTween.Sequence();
        
        // ------- Spawn
        //movement 
        m_parent.DOLocalMoveY(m_linkedConfig.YOffset, m_linkedConfig.spawnDuration + m_linkedConfig.stayDuration).SetRelative();
        if (m_linkedConfig.randomXMovement)
            m_parent.DOLocalMoveX(Random.Range(m_linkedConfig.minMaxXOffset.x, m_linkedConfig.minMaxXOffset.y), m_linkedConfig.spawnDuration + m_linkedConfig.stayDuration).SetRelative();
        
        //fade in
        _seq.Append(text.DOFade(1f, m_linkedConfig.spawnDuration));
        //scale in
        if (m_linkedConfig.enableScaleIn)
            _seq.Join(m_parent.DOScale(1f, m_linkedConfig.spawnDuration).SetEase(m_linkedConfig.scaleInEase));
        
        // ------- Stay
        _seq.AppendInterval(m_linkedConfig.stayDuration);

        // ------- Despawn
        //fade out
        _seq.AppendInterval(0);
        if (m_linkedConfig.enableFadeOut)
            _seq.Join(text.DOFade(0f, m_linkedConfig.despawnDuration));
        //scale out
        if (m_linkedConfig.enableScaleOut)
            _seq.Join(m_parent.DOScale(0f, m_linkedConfig.despawnDuration));

        _seq.OnComplete(() =>
        {
            LeanPool.Despawn(this);
        });
    }
}
