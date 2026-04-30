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
    [SerializeField] private Vector2 m_spawnRangeOuter = new Vector2(5, 5);
    [SerializeField] private Vector2 m_spawnRangeMin   = new Vector2(2, 2);
    [SerializeField] private bool m_spawnRangeGizmos = true;
    [SerializeField, Required] private EnemyScaler m_enemyScaler;

    private HashSet<Entity> m_roundEnemies = new HashSet<Entity>();
    public HashSet<Entity> RoundEnemies => m_roundEnemies;


    // m_playerPrefab.TryGetModule(out m_statModule);
    // m_statModule.GetValue(StatType.SpawnWeight);


    public void SpawnRound(int roundNumber)
    {
        // this.Log($"Spawning round {roundNumber}. Current enemy prefabs: {string.Join(", ", m_enemyPrefabs)}, count: {Mathf.Max(1, (int)StatManager.Instance.GetDefinitionValue(EntityType.Player, StatType.EnemiesPerWave))}");
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
            Vector3 spawnPos = CusRandom.RectangleMin(m_spawnRangeOuter, m_spawnRangeMin);
            StartCoroutine(SpawnEnemyCR(m_enemyPrefabs[enemyIndex], spawnPos, roundNumber));
        }
        // this.Log($"Spawned {count} enemies for round {roundNumber}. Enemies: {string.Join(", ", m_roundEnemies)}");
    }


    private IEnumerator SpawnEnemyCR(Entity prefab, Vector3 spawnPos, int roundNumber)
    {
        EntitySpawnModule spawnModule = prefab.GetComponent<EntitySpawnModule>();

        if (spawnModule != null && spawnModule.SpawnVFXPrefab != null)
        {
            ParticleSystem vfxInstance = LeanPool.Spawn(spawnModule.SpawnVFXPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(spawnModule.PreSpawnDelay);

            Entity enemy = LeanPool.Spawn(prefab, spawnPos, Quaternion.identity);
            m_enemyScaler.ApplyScaling(enemy, roundNumber);
            m_roundEnemies.Add(enemy);

            if (enemy.TryGetModule(out EntitySpawnModule enemySpawnModule))
                enemySpawnModule.SetSpawnVFXInstance(vfxInstance);
        }
        else
        {
            Entity enemy = LeanPool.Spawn(prefab, spawnPos, Quaternion.identity);
            m_enemyScaler.ApplyScaling(enemy, roundNumber);
            m_roundEnemies.Add(enemy);
            yield break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!m_spawnRangeGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_spawnRangeOuter.x * 2, 0.1f, m_spawnRangeOuter.y * 2));

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_spawnRangeMin.x * 2, 0.1f, m_spawnRangeMin.y * 2));
    }
}