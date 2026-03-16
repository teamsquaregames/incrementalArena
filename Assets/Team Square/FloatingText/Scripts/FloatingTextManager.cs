using Lean.Pool;
using MyBox;
using Pinpin;
using UnityEngine;
using UnityEngine.Serialization;

public class FloatingTextManager : Singleton<FloatingTextManager>
{
    [FormerlySerializedAs("m_uiFloatingTextPoolRef")] [SerializeField] private UIFloatingText m_uiFloatingTextPrefab;
    [FormerlySerializedAs("m_worldFloatingTextPoolRef")] [SerializeField] private WorldFloatingText m_worldFloatingTextPrefab;
    [SerializeField] private FloatingTextConfig m_defaultConfig;

    public void SpawnUIText(Vector3 screenPos, string text, FloatingTextConfig config)
    {
        UIFloatingText spawnedText = LeanPool.Spawn(m_uiFloatingTextPrefab, screenPos, Quaternion.identity);
        spawnedText.Init(text, config);
        spawnedText.Play();
    }
    
    public void SpawnWorldText(Vector3 worldpos, string text, FloatingTextConfig config = null)
    {
        WorldFloatingText spawnedText = LeanPool.Spawn(m_worldFloatingTextPrefab, worldpos, Quaternion.identity);
        spawnedText.Init(text, config != null ? config : m_defaultConfig);
        spawnedText.Play();
    }
}