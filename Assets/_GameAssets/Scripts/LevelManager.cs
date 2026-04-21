
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

    public bool IsWaveActive { get; private set; }

    private int m_currentRound = 0;
    private EntityHealthModule m_playerHealthModule;
    private HashSet<Entity> m_waveEnemies => m_spawnManager.RoundEnemies;
    private EndRoundUIC endRoundUIC => UIManager.Instance.GetCanvas<GameCanvas>().GetContainer<EndRoundUIC>();

    private void Awake()
    {
        GameManager.Instance.onRunStart += OnRunStart;
        EntityManager.Instance.onEntityUnregistered += OnEntityUnregistered;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onRunStart -= OnRunStart;
        EntityManager.Instance.onEntityUnregistered -= OnEntityUnregistered;
    }


    #region  Run

    private IEnumerator StartRound()
    {
        m_currentRound++;

        endRoundUIC.SetAnnouncementText($"Round {m_currentRound}");
        endRoundUIC.Open();

        yield return new WaitForSeconds(2);

        IsWaveActive = true;
        m_spawnManager.SpawnRound(m_currentRound);
    }

    private IEnumerator OnWaveComplete()
    {
        IsWaveActive = false;
        m_crowdRewards.SpawnRewards();
        m_crowdManager.CrowdCheer();

        yield return new WaitForSeconds(3);

        m_crowdRewards.CollectAllRewards();

        yield return new WaitForSeconds(1);
        StartCoroutine(StartRound());
    }

    private void OnRunStart()
    {
        IsWaveActive = false;
        m_currentRound = 0;
        m_waveEnemies.Clear();
        Entity player = LeanPool.Spawn(m_playerPrefab);

        if (player.TryGetModule(out m_playerHealthModule))
            m_playerHealthModule.OnDeath += OnPlayerDeath;

        DOVirtual.DelayedCall(3f, () =>
        {
            StartCoroutine(StartRound());
        });
    }

    #endregion

    private void OnEntityUnregistered(Entity entity)
    {
        // this.Log($"Entity unregistered: {entity}. Remaining enemies: {m_waveEnemies.Count}");
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
            EndRun();
    }

    private void OnPlayerDeath()
    {
        EndRun();
    }

    public void EndRun()
    {
        m_playerHealthModule.OnDeath -= OnPlayerDeath;

        m_crowdRewards.GrantAllPendingGold();

        UIManager.Instance.GetCanvas<RunEndCanvas>().Open();

        foreach (TrackedValueType type in GameConfig.Instance.gameSettings.trackedValuesToResetOnRunEnd)
            GameData.Instance.ResetTrackedValue(type);
    }


}
