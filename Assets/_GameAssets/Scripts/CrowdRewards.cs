using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;

[System.Serializable]
public class RewardItem
{
    [HorizontalGroup("Item", Width = 80)]
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public GameObject prefab;
    
    [HorizontalGroup("Item")]
    [VerticalGroup("Item/Props")]
    [LabelWidth(60)]
    public float value = 1f;
    
    [VerticalGroup("Item/Props")]
    [LabelWidth(60)]
    [Range(0f, 100f)]
    public float weight = 1f;
}

public class CrowdRewards : MonoBehaviour
{
    [TitleGroup("Reward Settings")]
    [SerializeField] private List<RewardItem> rewardItems = new List<RewardItem>();
    
    [TitleGroup("Spawn Settings")]
    [SerializeField] private float m_totalRewardValue = 100f;
    [SerializeField] private float m_spawnDuration = 2f;
    
    [TitleGroup("Spawn Zone (Rectangle)")]
    [SerializeField] private Vector3 m_spawnCenterOffset = Vector3.up;
    [SerializeField] private Vector3 m_spawnAreaSize = new Vector3(5f, 3f, 5f);
    [SerializeField] private bool m_spawnOnEdgesOnly = true;
    [SerializeField, ShowIf(nameof(m_spawnOnEdgesOnly))] private float m_edgeThickness = 0.5f;
    
    [TitleGroup("Physics")]
    [SerializeField] private float m_centerVelocityStrength = 4f;
    [SerializeField] private Vector2 m_velocityRangeX = new Vector2(-3f, 3f);
    [SerializeField] private Vector2 m_velocityRangeY = new Vector2(2f, 5f);
    [SerializeField] private Vector2 m_velocityRangeZ = new Vector2(-3f, 3f);
    [SerializeField] private bool m_addRandomTorque = true;
    [SerializeField, ShowIf(nameof(m_addRandomTorque))] private float m_torqueStrength = 10f;
    
    [TitleGroup("Debug")]
    [SerializeField] private bool m_showGizmos = true;
    
    private float m_totalWeight;
    private List<GameObject> m_spawnedRewards = new List<GameObject>();
    
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void SpawnRewards()
    {
        ClearPreviousRewards();
        StartCoroutine(SpawnRewardsCoroutine());
    }
    
    private IEnumerator SpawnRewardsCoroutine()
    {
        if (rewardItems.Count == 0)
        {
            Debug.LogWarning("No reward items configured!");
            yield break;
        }
        
        CalculateTotalWeight();
        
        float remainingValue = m_totalRewardValue;
        float elapsedTime = 0f;
        
        while (remainingValue > 0f && elapsedTime < m_spawnDuration)
        {
            // Sélectionne un reward pondéré
            RewardItem selectedReward = SelectWeightedReward();
            
            if (selectedReward == null || selectedReward.prefab == null)
            {
                yield break;
            }
            
            // Spawn le reward
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject reward = Instantiate(selectedReward.prefab, spawnPos, Random.rotation);
            
            // Applique la vélocité
            Rigidbody rb = reward.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 toCenter = transform.position - spawnPos;
                if (toCenter.sqrMagnitude < 0.0001f)
                {
                    toCenter = Vector3.forward;
                }

                Vector3 baseVelocity = toCenter.normalized * m_centerVelocityStrength;
                Vector3 randomVelocityOffset = new Vector3(
                    Random.Range(m_velocityRangeX.x, m_velocityRangeX.y),
                    Random.Range(m_velocityRangeY.x, m_velocityRangeY.y),
                    Random.Range(m_velocityRangeZ.x, m_velocityRangeZ.y)
                );
                rb.linearVelocity = baseVelocity + randomVelocityOffset;
                
                if (m_addRandomTorque)
                {
                    rb.angularVelocity = Random.insideUnitSphere * m_torqueStrength;
                }
            }
            
            m_spawnedRewards.Add(reward);
            
            // Déduis la valeur
            remainingValue -= selectedReward.value;
            
            // Attends un peu avant le prochain spawn
            float spawnInterval = m_spawnDuration / (m_totalRewardValue / GetAverageValue()) / 2;
            yield return new WaitForSeconds(spawnInterval * 0.5f);
            
            elapsedTime += spawnInterval * 0.5f;
        }
        
        Debug.Log($"Spawned {m_spawnedRewards.Count} rewards with total value ~{m_totalRewardValue - remainingValue:F1}");
    }
    
    private void CalculateTotalWeight()
    {
        m_totalWeight = 0f;
        foreach (var item in rewardItems)
        {
            m_totalWeight += item.weight;
        }
    }
    
    private RewardItem SelectWeightedReward()
    {
        if (m_totalWeight <= 0f) return rewardItems[0];
        
        float randomValue = Random.Range(0f, m_totalWeight);
        float cumulativeWeight = 0f;
        
        foreach (var item in rewardItems)
        {
            cumulativeWeight += item.weight;
            if (randomValue <= cumulativeWeight)
            {
                return item;
            }
        }
        
        return rewardItems[rewardItems.Count - 1];
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 basePos = transform.position + m_spawnCenterOffset;
        float randomY = Random.Range(-m_spawnAreaSize.y / 2f, m_spawnAreaSize.y / 2f);
        
        if (m_spawnOnEdgesOnly)
        {
            // Spawn sur les bords du rectangle
            int edge = Random.Range(0, 4); // 0=top, 1=right, 2=bottom, 3=left
            
            switch (edge)
            {
                case 0: // Top
                    return basePos + new Vector3(
                        Random.Range(-m_spawnAreaSize.x / 2f, m_spawnAreaSize.x / 2f),
                        randomY,
                        m_spawnAreaSize.z / 2f + Random.Range(0f, m_edgeThickness)
                    );
                case 1: // Right
                    return basePos + new Vector3(
                        m_spawnAreaSize.x / 2f + Random.Range(0f, m_edgeThickness),
                        randomY,
                        Random.Range(-m_spawnAreaSize.z / 2f, m_spawnAreaSize.z / 2f)
                    );
                case 2: // Bottom
                    return basePos + new Vector3(
                        Random.Range(-m_spawnAreaSize.x / 2f, m_spawnAreaSize.x / 2f),
                        randomY,
                        -m_spawnAreaSize.z / 2f - Random.Range(0f, m_edgeThickness)
                    );
                default: // Left
                    return basePos + new Vector3(
                        -m_spawnAreaSize.x / 2f - Random.Range(0f, m_edgeThickness),
                        randomY,
                        Random.Range(-m_spawnAreaSize.z / 2f, m_spawnAreaSize.z / 2f)
                    );
            }
        }
        else
        {
            // Spawn dans toute la zone
            return basePos + new Vector3(
                Random.Range(-m_spawnAreaSize.x / 2f, m_spawnAreaSize.x / 2f),
                randomY,
                Random.Range(-m_spawnAreaSize.z / 2f, m_spawnAreaSize.z / 2f)
            );
        }
    }
    
    private float GetAverageValue()
    {
        if (rewardItems.Count == 0) return 1f;
        
        float weightedSum = 0f;
        foreach (var item in rewardItems)
        {
            weightedSum += item.value * item.weight;
        }
        
        return weightedSum / m_totalWeight;
    }
    
    [Button("Clear Rewards"), GUIColor(1f, 0.5f, 0.5f)]
    private void ClearPreviousRewards()
    {
        foreach (var reward in m_spawnedRewards)
        {
            if (reward != null)
            {
                Destroy(reward);
            }
        }
        m_spawnedRewards.Clear();
    }
    
    private void OnDrawGizmos()
    {
        if (!m_showGizmos) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + m_spawnCenterOffset;
        
        // Zone 3D
        Gizmos.DrawWireCube(center, m_spawnAreaSize);
        
        // Edge thickness si activé
        if (m_spawnOnEdgesOnly)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            
            // Top edge (Z+)
            Gizmos.DrawCube(center + new Vector3(0, 0, m_spawnAreaSize.z / 2f + m_edgeThickness / 2f),
                new Vector3(m_spawnAreaSize.x, m_spawnAreaSize.y, m_edgeThickness));
            // Right edge (X+)
            Gizmos.DrawCube(center + new Vector3(m_spawnAreaSize.x / 2f + m_edgeThickness / 2f, 0, 0),
                new Vector3(m_edgeThickness, m_spawnAreaSize.y, m_spawnAreaSize.z));
            // Bottom edge (Z-)
            Gizmos.DrawCube(center + new Vector3(0, 0, -m_spawnAreaSize.z / 2f - m_edgeThickness / 2f),
                new Vector3(m_spawnAreaSize.x, m_spawnAreaSize.y, m_edgeThickness));
            // Left edge (X-)
            Gizmos.DrawCube(center + new Vector3(-m_spawnAreaSize.x / 2f - m_edgeThickness / 2f, 0, 0),
                new Vector3(m_edgeThickness, m_spawnAreaSize.y, m_spawnAreaSize.z));
        }
    }
    
    private void OnDestroy()
    {
        ClearPreviousRewards();
    }
}
