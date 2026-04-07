
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    
    [Header("Scene references")]
    [SerializeField] private CrowdRewards m_crowdRewards;

    private int m_currentWave = 0;
    private HashSet<Entity> m_waveEnemies = new HashSet<Entity>();
    private RunTimerUIC m_runTimerUIC => UIManager.Instance.GetCanvas<GameCanvas>().GetContainer<RunTimerUIC>();

    private void Awake()
    {
        GameManager.Instance.onRunTimerStart += OnRunTimerStart;
        EntityManager.Instance.onEntityUnregistered += OnEntityUnregistered;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.onRunTimerStart -= OnRunTimerStart;
    }



    #region  Run

    private void StartWave()
    {
        m_currentWave++;
        int count = Mathf.Max(1, (int)StatManager.Instance.GetDefinitionValue(EntityType.Player, StatType.EnemiesPerWave));

        for (int i = 0; i < count; i++)
        {
            Entity enemy = LeanPool.Spawn(m_enemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
            m_waveEnemies.Add(enemy);
        }
        
        m_runTimerUIC.SetTimerPause(false);
    }

    private IEnumerator OnWaveComplete()
    {
        m_runTimerUIC.SetTimerPause(true);
        m_crowdRewards.SpawnRewards();
        
        yield return new WaitForSeconds(5);
        
        StartWave();
    }
    
    private void OnRunTimerStart()
    {
        m_currentWave = 0;
        m_waveEnemies.Clear();
        LeanPool.Spawn(m_playerPrefab);

        DOVirtual.DelayedCall(3f, () =>
        {
            StartWave();
        });
    }

    #endregion



    private void OnEntityUnregistered(Entity entity)
    {
        if (!m_waveEnemies.Remove(entity)) return;

        if (m_waveEnemies.Count == 0)
        {
            StartCoroutine(OnWaveComplete());
        }
    }
}
