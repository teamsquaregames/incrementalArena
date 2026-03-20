
using System;
using Lean.Pool;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{
    public Entity m_playerPrefab;
    public Entity m_enemyPrefab;
    public int m_enemyCount = 5;
    [SerializeField] private bool m_useConstantEnemyCount = true;

    private void Start()
    {
        //spawn player

        if (m_useConstantEnemyCount)
        {
            EntityManager.Instance.onEntityUnregistered += ConstantEnemyCount;
        }

        LeanPool.Spawn(m_playerPrefab);
        for (int i = 0; i < m_enemyCount; i++)
        {
            LeanPool.Spawn(m_enemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
        }
    }

    private void ConstantEnemyCount(Entity entity)
    {
        this.Log("Enemy died, spawning a new one to keep the count constant");
        LeanPool.Spawn(m_enemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
    }
}