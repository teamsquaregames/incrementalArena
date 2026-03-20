
using System.Collections.Generic;
using Lean.Pool;
using MyBox;
using Stats;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{
    public Entity m_playerPrefab;
    public Entity m_enemyPrefab;

    private int m_currentWave = 0;
    private HashSet<Entity> m_waveEnemies = new HashSet<Entity>();

    private void Start()
    {
        LeanPool.Spawn(m_playerPrefab);
        EntityManager.Instance.onEntityUnregistered += OnEntityUnregistered;
        StartWave();
    }

    private void StartWave()
    {
        m_currentWave++;
        int count = Mathf.Max(1, (int)StatManager.Instance.GetDefinitionValue(EntityType.Player, StatType.EnemiesPerWave));

        for (int i = 0; i < count; i++)
        {
            Entity enemy = LeanPool.Spawn(m_enemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
            m_waveEnemies.Add(enemy);
        }
    }

    private void OnEntityUnregistered(Entity entity)
    {
        if (!m_waveEnemies.Remove(entity)) return;

        if (m_waveEnemies.Count == 0)
            StartWave();
    }
}
