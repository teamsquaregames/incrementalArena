using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using MyBox;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Random = UnityEngine.Random;

public class SpawnManager : Singleton<SpawnManager>
{
    [TitleGroup("Settings")]
    [SerializeField] private List<Entity> m_enemyPrefabs;
    [SerializeField] private Vector2 m_spawnRange = new Vector2(5, 5);
    [SerializeField] private bool m_spawnRangeGizmos = true;
    [SerializeField, Required] private EnemyScaler m_enemyScaler;

    private HashSet<Entity> m_roundEnemies = new HashSet<Entity>();
    public HashSet<Entity> RoundEnemies => m_roundEnemies;


    // m_playerPrefab.TryGetModule(out m_statModule);
    // m_statModule.GetValue(StatType.SpawnWeight);


    public void SpawnRound(int roundNumber)
    {
        int count = Mathf.Max(1, (int)StatManager.Instance.GetDefinitionValue(EntityType.Player, StatType.EnemiesPerWave));

        if (!GameConfig.Instance.cheatSettings.usePrefabEnemies)
            m_enemyPrefabs = GameData.Instance.unlockedEnemies;

        float[] spawnWeight = new float[m_enemyPrefabs.Count];
        for (int i = 0; i < m_enemyPrefabs.Count; i++)
        {
            spawnWeight[i] = StatManager.Instance.GetDefinitionValue(m_enemyPrefabs[i].EntityType, StatType.SpawnWeight);
        }

        // Debug.Log($"Spawning round {roundNumber} with {count} enemies. Spawn weights: {string.Join(", ", spawnWeight)}");

        for (int i = 0; i < count; i++)
        {
            int enemyIndex = CusRandom.RandomWeighted(spawnWeight);
            Vector3 spawnPos = new Vector3(
                Random.Range(-m_spawnRange.x, m_spawnRange.x),
                0f,
                Random.Range(-m_spawnRange.y, m_spawnRange.y));
            Entity enemy = LeanPool.Spawn(m_enemyPrefabs[enemyIndex], spawnPos, Quaternion.identity);
            m_enemyScaler.ApplyScaling(enemy, roundNumber);
            m_roundEnemies.Add(enemy);
        }
    }


    /// spawn range gizmos
    private void OnDrawGizmosSelected()
    {
        if (m_spawnRangeGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_spawnRange.x * 2, 0.1f, m_spawnRange.y * 2));
        }
    }
}