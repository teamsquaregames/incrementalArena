
using System;
using Lean.Pool;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : Singleton<LevelManager>
{
    public Entity m_playerPrefab;

    private void Start()
    {
        //spawn player
        LeanPool.Spawn(m_playerPrefab);
    }

    private void StartRound()
    {
        //spawn enemies
    }
} 