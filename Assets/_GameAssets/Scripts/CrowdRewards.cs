using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Lean.Pool;
using Sirenix.OdinInspector;

[System.Serializable]
public class CrowdRewards : MonoBehaviour
{
    [TitleGroup("Spawn Settings")]
    [SerializeField] private float m_spawnDuration = 2f;
    
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)")] private Vector3 m_spawnCenterOffset = Vector3.up;
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)")] private Vector3 m_spawnAreaSize = new Vector3(5f, 3f, 5f);
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)")] private bool m_spawnOnEdgesOnly = true;
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)"), ShowIf(nameof(m_spawnOnEdgesOnly))] private float m_edgeThickness = 0.5f;
    
    [SerializeField, FoldoutGroup("Physics")] private float m_targetRadius = 3f;
    [SerializeField, FoldoutGroup("Physics")] private float m_launchAngle = 45f;
    [SerializeField, FoldoutGroup("Physics")] private bool m_addRandomTorque = true;
    [SerializeField, FoldoutGroup("Physics"), ShowIf(nameof(m_addRandomTorque))] private float m_torqueStrength = 10f;

    [TitleGroup("Floating Text")]
    [SerializeField] private FloatingTextConfig m_collectFloatingTextConfig;

    [TitleGroup("Debug")]
    [SerializeField] private bool m_showGizmos = true;

    private List<RewardObject> m_spawnedRewards = new List<RewardObject>();
    private List<RewardEntry> m_rewardEntries = new List<RewardEntry>();

    public void AddRewardEntry(RewardEntry entry)
    {
        m_rewardEntries.Add(entry);
    }

    public void RegisterReward(RewardObject reward)
    {
        m_spawnedRewards.Add(reward);
    }

    public void UnregisterReward(RewardObject reward)
    {
        m_spawnedRewards.Remove(reward);
    }

    public void GrantAllPendingGold()
    {
        foreach (RewardEntry entry in m_rewardEntries)
        {
            if (entry.rewardObject == null || entry.rewardObject.RewardConfig?.CurrencyAsset == null) continue;
            GameData.Instance.AddCurrency(entry.rewardObject.RewardConfig.CurrencyAsset, entry.value);
        }
        m_rewardEntries.Clear();

        var spawnedCopy = new List<RewardObject>(m_spawnedRewards);
        foreach (RewardObject reward in spawnedCopy)
        {
            if (reward == null || reward.RewardConfig?.CurrencyAsset == null) continue;
            GameData.Instance.AddCurrency(reward.RewardConfig.CurrencyAsset, reward.Value);
            LeanPool.Despawn(reward);
        }
        m_spawnedRewards.Clear();
    }

    public void CollectAllRewards()
    {
        var rewardsToCollect = new List<RewardObject>(m_spawnedRewards);

        // Sum total value per CurrencyAsset across all pending rewards
        var totals = new Dictionary<CurrencyAsset, double>();
        foreach (var reward in rewardsToCollect)
        {
            if (reward == null || reward.RewardConfig == null || reward.RewardConfig.CurrencyAsset == null) continue;
            CurrencyAsset asset = reward.RewardConfig.CurrencyAsset;
            totals.TryGetValue(asset, out double current);
            totals[asset] = current + reward.Value;
        }

        // Show one floating text per currency at the player's position
        Vector3 textPos = EntityManager.Instance?.Player?.transform.position ?? Vector3.zero;
        foreach (var kvp in totals)
        {
            string text = $"+{kvp.Value:N0} {kvp.Key.SpriteAssetString}";
            FloatingTextManager.Instance?.SpawnWorldText(textPos, text, m_collectFloatingTextConfig);
        }

        // Collect all without individual floating texts
        foreach (var reward in rewardsToCollect)
        {
            if (reward != null)
                reward.PickUp(showFloatingText: false);
        }
        
        m_rewardEntries.Clear();
    }

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void SpawnRewards()
    {
        ClearPreviousRewards();
        StartCoroutine(SpawnRewardsCoroutine());
    }

    private IEnumerator SpawnRewardsCoroutine()
    {
        if (m_rewardEntries.Count == 0)
        {
            Debug.LogWarning("CrowdRewards: No reward entries to spawn.");
            yield break;
        }

        m_rewardEntries.AddRange(m_rewardEntries);
        m_rewardEntries.AddRange(m_rewardEntries);
        m_rewardEntries.AddRange(m_rewardEntries);

        float interval = m_spawnDuration / m_rewardEntries.Count;

        for (int i = 0; i < m_rewardEntries.Count; i++)
        {
            SpawnReward(m_rewardEntries[i]);
            if (i < m_rewardEntries.Count - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnReward(RewardEntry entry)
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        RewardObject reward = LeanPool.Spawn(entry.rewardObject, spawnPos, Random.rotation);
        reward.SetValue(entry.value);

        Vector2 randomCircle = Random.insideUnitCircle * m_targetRadius;
        Vector3 target = new Vector3(randomCircle.x, 0f, randomCircle.y);

        Vector3 launchVelocity = ComputeLaunchVelocity(spawnPos, target, m_launchAngle);
        reward.Launch(launchVelocity, m_addRandomTorque ? m_torqueStrength : 0f);
    }

    private Vector3 ComputeLaunchVelocity(Vector3 from, Vector3 to, float angleDeg)
    {
        Vector3 delta = to - from;
        Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);
        float d = deltaXZ.magnitude;
        float deltaH = delta.y;
        float g = Mathf.Abs(Physics.gravity.y);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float denom = 2f * Mathf.Cos(angleRad) * Mathf.Cos(angleRad) * (d * Mathf.Tan(angleRad) - deltaH);

        if (denom <= 0f || d < 0.001f)
        {
            Debug.LogWarning($"CrowdRewards: invalid launch angle {angleDeg}° for this trajectory (denom={denom:F3}), using fallback.");
            return (delta.normalized + Vector3.up) * 5f;
        }

        float v = Mathf.Sqrt(g * d * d / denom);
        return deltaXZ.normalized * (v * Mathf.Cos(angleRad)) + Vector3.up * (v * Mathf.Sin(angleRad));
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 basePos = transform.position + m_spawnCenterOffset;
        float randomY = Random.Range(-m_spawnAreaSize.y / 2f, m_spawnAreaSize.y / 2f);

        if (m_spawnOnEdgesOnly)
        {
            int edge = Random.Range(0, 4);
            switch (edge)
            {
                case 0: // Top
                    return basePos + new Vector3(
                        Random.Range(-m_spawnAreaSize.x / 2f, m_spawnAreaSize.x / 2f),
                        randomY,
                        m_spawnAreaSize.z / 2f + Random.Range(0f, m_edgeThickness));
                case 1: // Right
                    return basePos + new Vector3(
                        m_spawnAreaSize.x / 2f + Random.Range(0f, m_edgeThickness),
                        randomY,
                        Random.Range(-m_spawnAreaSize.z / 2f, m_spawnAreaSize.z / 2f));
                case 2: // Bottom
                    return basePos + new Vector3(
                        Random.Range(-m_spawnAreaSize.x / 2f, m_spawnAreaSize.x / 2f),
                        randomY,
                        -m_spawnAreaSize.z / 2f - Random.Range(0f, m_edgeThickness));
                default: // Left
                    return basePos + new Vector3(
                        -m_spawnAreaSize.x / 2f - Random.Range(0f, m_edgeThickness),
                        randomY,
                        Random.Range(-m_spawnAreaSize.z / 2f, m_spawnAreaSize.z / 2f));
            }
        }

        return basePos + new Vector3(
            Random.Range(-m_spawnAreaSize.x / 2f, m_spawnAreaSize.x / 2f),
            randomY,
            Random.Range(-m_spawnAreaSize.z / 2f, m_spawnAreaSize.z / 2f));
    }

    [Button("Clear Rewards"), GUIColor(1f, 0.5f, 0.5f)]
    private void ClearPreviousRewards()
    {
        foreach (var reward in m_spawnedRewards)
        {
            if (reward != null)
                LeanPool.Despawn(reward);
        }
        m_spawnedRewards.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!m_showGizmos) return;

        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + m_spawnCenterOffset;

        Gizmos.DrawWireCube(center, m_spawnAreaSize);

        if (m_spawnOnEdgesOnly)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

            Gizmos.DrawCube(center + new Vector3(0, 0, m_spawnAreaSize.z / 2f + m_edgeThickness / 2f),
                new Vector3(m_spawnAreaSize.x, m_spawnAreaSize.y, m_edgeThickness));
            Gizmos.DrawCube(center + new Vector3(m_spawnAreaSize.x / 2f + m_edgeThickness / 2f, 0, 0),
                new Vector3(m_edgeThickness, m_spawnAreaSize.y, m_spawnAreaSize.z));
            Gizmos.DrawCube(center + new Vector3(0, 0, -m_spawnAreaSize.z / 2f - m_edgeThickness / 2f),
                new Vector3(m_spawnAreaSize.x, m_spawnAreaSize.y, m_edgeThickness));
            Gizmos.DrawCube(center + new Vector3(-m_spawnAreaSize.x / 2f - m_edgeThickness / 2f, 0, 0),
                new Vector3(m_edgeThickness, m_spawnAreaSize.y, m_spawnAreaSize.z));
        }

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.8f);
        DrawWireCircle(Vector3.zero, m_targetRadius, 32);
    }

    private void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float step = 2f * Mathf.PI / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private void OnDestroy()
    {
        ClearPreviousRewards();
    }
}
