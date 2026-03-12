
using System;
using Lean.Pool;
using MyBox;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] private EntityPoolRef m_playerPoolRef;
    [SerializeField] private EntityPoolRef m_enemyPoolRef;

    private void Start()
    {
        //spawn player
    }

    private void StartRound()
    {
        //spawn enemies
    }
}