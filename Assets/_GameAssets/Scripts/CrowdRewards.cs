using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[System.Serializable]
public class CrowdRewards : MonoBehaviour
{
    [FormerlySerializedAs("m_rewardEntries")]
    [TitleGroup("Reward Settings")]
    [SerializeField] private List<RewardObject> m_rewardObjects = new List<RewardObject>();

    [TitleGroup("Spawn Settings")]
    [SerializeField] private float m_spawnDuration = 2f;
    
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)")] private Vector3 m_spawnCenterOffset = Vector3.up;
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)")] private Vector3 m_spawnAreaSize = new Vector3(5f, 3f, 5f);
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)")] private bool m_spawnOnEdgesOnly = true;
    [SerializeField, FoldoutGroup("Spawn Zone (Rectangle)"), ShowIf(nameof(m_spawnOnEdgesOnly))] private float m_edgeThickness = 0.5f;
    
    [SerializeField, FoldoutGroup("Physics")] private float m_centerVelocityStrength = 4f;
    [SerializeField, FoldoutGroup("Physics")] private Vector2 m_velocityRangeX = new Vector2(-3f, 3f);
    [SerializeField, FoldoutGroup("Physics")] private Vector2 m_velocityRangeY = new Vector2(2f, 5f);
    [SerializeField, FoldoutGroup("Physics")] private Vector2 m_velocityRangeZ = new Vector2(-3f, 3f);
    [SerializeField, FoldoutGroup("Physics")] private bool m_addRandomTorque = true;
    [SerializeField, FoldoutGroup("Physics"), ShowIf(nameof(m_addRandomTorque))] private float m_torqueStrength = 10f;

    [TitleGroup("Debug")]
    [SerializeField] private bool m_showGizmos = true;

    private List<RewardObject> m_spawnedRewards = new List<RewardObject>();

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void SpawnRewards()
    {
        ClearPreviousRewards();
        StartCoroutine(SpawnRewardsCoroutine());
    }

    private IEnumerator SpawnRewardsCoroutine()
    {
        int nbToSpawn = 100;
        float interval = m_spawnDuration / nbToSpawn;

        for (int i = 0; i < nbToSpawn; i++)
        {
            SpawnReward(m_rewardObjects[0]);
            if (i < nbToSpawn - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnReward(RewardObject prefab)
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        RewardObject reward = LeanPool.Spawn(prefab, spawnPos, Random.rotation);
        reward.SetValue(1);

        Rigidbody rb = reward.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 toCenter = transform.position - spawnPos;
            if (toCenter.sqrMagnitude < 0.0001f) toCenter = Vector3.forward;

            Vector3 baseVelocity = toCenter.normalized * m_centerVelocityStrength;
            Vector3 randomOffset = new Vector3(
                Random.Range(m_velocityRangeX.x, m_velocityRangeX.y),
                Random.Range(m_velocityRangeY.x, m_velocityRangeY.y),
                Random.Range(m_velocityRangeZ.x, m_velocityRangeZ.y)
            );
            rb.linearVelocity = baseVelocity + randomOffset;

            if (m_addRandomTorque)
                rb.angularVelocity = Random.insideUnitSphere * m_torqueStrength;
        }

        m_spawnedRewards.Add(reward);
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
    }

    private void OnDestroy()
    {
        ClearPreviousRewards();
    }
}
