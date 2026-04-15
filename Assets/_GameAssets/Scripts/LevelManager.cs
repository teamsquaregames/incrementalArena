
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

    [Header("Dependencies")]
    [SerializeField, Required] private CrowdRewards m_crowdRewards;
    [SerializeField, Required] private CrowdManager m_crowdManager;
    [SerializeField, Required] private SpawnManager m_spawnManager;

    
    public CrowdRewards CrowdRewards => m_crowdRewards;

    private int m_currentRound = 0;
    private EntityHealthModule m_playerHealthModule;
    private RunTimerUIC m_runTimerUIC => UIManager.Instance.GetCanvas<GameCanvas>().GetContainer<RunTimerUIC>();
    private HashSet<Entity> m_waveEnemies => m_spawnManager.RoundEnemies;

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

    private void StartRound()
    {
        m_currentRound++;

        m_spawnManager.SpawnRound(m_currentRound);

        m_runTimerUIC.SetTimerPause(false);
    }

    private IEnumerator OnWaveComplete()
    {
        m_runTimerUIC.SetTimerPause(true);
        m_crowdRewards.SpawnRewards();
        m_crowdManager.CrowdCheer();

        yield return new WaitForSeconds(5);

        m_crowdRewards.CollectAllRewards();
        StartRound();
    }

    private void OnRunTimerStart()
    {
        m_currentRound = 0;
        m_waveEnemies.Clear();
        Entity player = LeanPool.Spawn(m_playerPrefab);

        if (player.TryGetModule(out m_playerHealthModule))
            m_playerHealthModule.OnDeath += OnPlayerDeath;

        DOVirtual.DelayedCall(3f, () =>
        {
            StartRound();
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


}
