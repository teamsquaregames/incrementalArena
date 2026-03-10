using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntitySheenModule : EntityModule
{
    [SerializeField, Min(0f)] private float sheenHoldDuration = 0.05f;
    [SerializeField, Min(0f)] private float sheenFadeDuration = 0.15f;
    [SerializeField] private Renderer[] m_renderers;

    private Material[] m_materials;
    private Color[] m_originalColors;
    private Sequence m_sheenSequence;

    private const string EMISSION_COLOR_PROPERTY = "_EmissionColor";

    protected override void OnInitialize()
    {
        base.OnInitialize();

        m_materials = new Material[m_renderers.Length];
        m_originalColors = new Color[m_renderers.Length];

        for (int i = 0; i < m_renderers.Length; i++)
        {
            if (m_renderers[i] == null) continue;

            Material mat = m_renderers[i].material;
            m_materials[i] = mat;

            mat.EnableKeyword("_EMISSION");

            m_originalColors[i] = mat.HasProperty(EMISSION_COLOR_PROPERTY)
                ? mat.GetColor(EMISSION_COLOR_PROPERTY)
                : Color.black;
        }
    }

    [Button]
    private void CacheRenderers()
    {
        m_renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    public void PlayWhiteSheen()
    {
        m_sheenSequence?.Kill(complete: false);
        m_sheenSequence = DOTween.Sequence().SetLink(Owner.gameObject);

        for (int i = 0; i < m_renderers.Length; i++)
        {
            if (m_renderers[i] == null) continue;

            Material mat = m_materials[i];
            if (mat == null || !mat.HasProperty(EMISSION_COLOR_PROPERTY)) continue;

            mat.SetColor(EMISSION_COLOR_PROPERTY, Color.white);

            Tween fade = mat
                .DOColor(m_originalColors[i], EMISSION_COLOR_PROPERTY, sheenFadeDuration)
                .SetEase(Ease.OutQuad);

            m_sheenSequence.Insert(sheenHoldDuration, fade);
        }

        m_sheenSequence.Play();
    }
}