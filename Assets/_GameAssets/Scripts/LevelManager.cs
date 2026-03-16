
using System;
using Lean.Pool;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{
    public Entity m_playerPrefab;
    public Entity m_enemyPrefab;

    private void Start()
    {
        //spawn player
        LeanPool.Spawn(m_playerPrefab);
        for (int i = 0; i < 5; i++)
        {
            LeanPool.Spawn(m_enemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
        }
    }

    private void StartRound()
    {

    }
} 