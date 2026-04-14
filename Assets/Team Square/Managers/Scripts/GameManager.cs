using Sirenix.OdinInspector;
using System;
using Lean.Pool;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Action onRunTimerStart;
    public Action onRunTimerEnd;

    private bool m_isPaused = false;
    public bool IsPaused => m_isPaused;

    void Start()
    {
        TutorialManager.Instance.Init();

        if (!GameConfig.Instance.debuggingSettings.noMusic)
            SoundManager.Instance.PlayMusic(SoundKeys.music);
            
            SoundManager.Instance.PlayAmbient(SoundKeys.ambient);
    }

    public void StartRun()
    {
        onRunTimerStart?.Invoke();
    }

    public void EndRun()
    {
        onRunTimerEnd?.Invoke();
        QuitRun();
    }

    public void EnterRun()
    {
        UIManager.Instance.GetCanvas<GameCanvas>().Open();
        UIManager.Instance.GetCanvas<SkillTreeCanvas>().Close();
        UIManager.Instance.GetCanvas<EndRoundCanvas>().Close();

        SetPause(false);
        GameData.Instance.runActive = true;
        DespawnPooledObjectAndTuto();
        StartRun();
    }

    public void FadeAndEnterRun()
    {
        FadeManager.Instance.FadeInWithScene("MainScene", () =>
        {
            EnterRun();
        });
    }

    [Button]
    public void QuitRun()
    {
        UIManager.Instance.GetCanvas<GameCanvas>().Close();
        GameData.Instance.IncrementTrackedValue(TrackedValueType.RunCount, 1);
        CameraController.Instance.SetControl(false);
        GameData.Instance.runActive = false;

        FadeManager.Instance.FadeIn(() =>
        {
            DespawnPooledObjectAndTuto();
            GameData.Instance.ResetRun();
            UIManager.Instance.GetCanvas<SkillTreeCanvas>().Open();
        });
    }

    private void DespawnPooledObjectAndTuto()
    {
        LeanPool.DespawnAll();
        TutorialUIManager.Instance?.DespawnAllTutos();
    }

    public void SetPause(bool paused)
    {
        m_isPaused = paused;

        if (CameraController.Instance != null)
            CameraController.Instance.SetControl(!paused);
    }
}