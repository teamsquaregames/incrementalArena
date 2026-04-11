
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

public class LevelManager : Singleton<LevelManager>
{
    public Entity m_playerPrefab;
    public Entity m_enemyPrefab;

    [Header("Dependencies")]
    [SerializeField, Required] private CrowdRewards m_crowdRewards;
    [SerializeField, Required] private CrowdManager m_crowdManager;

    [TitleGroup("Settings")]
    [SerializeField] private Vector2 m_spawnRange = new Vector2(5, 5);
    [SerializeField] private bool m_spawnRangeGizmos = true;
    
    public CrowdRewards CrowdRewards => m_crowdRewards;

    private int m_currentWave = 0;
    private HashSet<Entity> m_waveEnemies = new HashSet<Entity>();
    private EntityHealthModule m_playerHealthModule;
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
            Entity enemy = LeanPool.Spawn(m_enemyPrefab, new Vector3(Random.Range(-m_spawnRange.x, m_spawnRange.x), 0, Random.Range(-m_spawnRange.y, m_spawnRange.y)), Quaternion.identity);
            m_waveEnemies.Add(enemy);
        }

        m_runTimerUIC.SetTimerPause(false);
    }

    private IEnumerator OnWaveComplete()
    {
        m_runTimerUIC.SetTimerPause(true);
        m_crowdRewards.SpawnRewards();
        m_crowdManager.CrowdCheer();

        yield return new WaitForSeconds(5);

        m_crowdRewards.CollectAllRewards();
        StartWave();
    }

    private void OnRunTimerStart()
    {
        m_currentWave = 0;
        m_waveEnemies.Clear();
        Entity player = LeanPool.Spawn(m_playerPrefab);

        if (player.TryGetModule(out m_playerHealthModule))
            m_playerHealthModule.OnDeath += OnPlayerDeath;

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

    private void Update()
    {
        if (!GameConfig.Instance.debuggingSettings.developmentBuild) return;
        if (Keyboard.current.endKey.wasPressedThisFrame)
            GameManager.Instance.EndRun();
    }

    private void OnPlayerDeath()
    {
        m_playerHealthModule.OnDeath -= OnPlayerDeath;
        GameManager.Instance.EndRun();
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
