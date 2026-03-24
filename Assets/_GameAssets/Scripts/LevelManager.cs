
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

    private float m_runDuration;
    private float m_timeRemaining;

    public float TimeRemaining => m_timeRemaining;
    public float RunDuration   => m_runDuration;

    private void Start()
    {
        GameManager.Instance.OnRunStart += OnRunStart;
        GameManager.Instance.OnRunEnd   += OnRunEnd;
        EntityManager.Instance.onEntityUnregistered += OnEntityUnregistered;
        
        //placeholder
        GameManager.Instance.StartRun();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnRunStart -= OnRunStart;
        GameManager.Instance.OnRunEnd   -= OnRunEnd;
    }

    private void Update()
    {
        if (!GameData.Instance.runActive) return;
        if (GameConfig.Instance.cheatSettings.infiniteRunDuration) return;

        m_timeRemaining -= Time.deltaTime;

        if (m_timeRemaining <= 0f)
        {
            m_timeRemaining = 0f;
            GameManager.Instance.EndRun();
        }
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
    }
    
    private void StartTimer()
    {
        float statValue = StatManager.Instance.GetDefinitionValue(EntityType.Player, StatType.RunDuration);
        m_runDuration   = statValue > 0f ? statValue : 60f;
        m_timeRemaining = m_runDuration;
    }
    
    private void OnRunStart()
    {
        this.Log("On run start");
        m_currentWave = 0;
        m_waveEnemies.Clear();
        LeanPool.Spawn(m_playerPrefab);
        
        StartWave();
        StartTimer();
    }

    private void OnRunEnd()
    {
        this.Log("On run end");
    }

    #endregion



    private void OnEntityUnregistered(Entity entity)
    {
        if (!m_waveEnemies.Remove(entity)) return;

        if (m_waveEnemies.Count == 0)
            StartWave();
    }
}
