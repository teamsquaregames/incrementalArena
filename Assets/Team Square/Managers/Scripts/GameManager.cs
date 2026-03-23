using Sirenix.OdinInspector;
using UnityEngine;
using System;
using Lean.Pool;
using MyBox;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Action OnRunStart;
    public Action OnRunEnd; // == fired quand le timer arrive à 0
    
    [TitleGroup("Dependencies")]
    private GameConfig m_gameConfig;
    private GameData m_gameData;
    
    private bool m_isPaused = false;
    public bool IsPaused => m_isPaused;
    
    void Start()
    {
        m_gameConfig = GameConfig.Instance;
        m_gameData = GameData.Instance;
        
        TutorialManager.Instance.Init();
    
        SoundManager.Instance.PlayMusic(SoundKeys.music);
        SoundManager.Instance.PlayAmbient(SoundKeys.ambient);
    }
    
    [Button]
    public void EndRun()
    {
        GameData.Instance.IncrementTrackedValue(TrackedValueType.RunCount, 1);
        CameraController.Instance.SetControl(false);
    
        FadeManager.Instance.FadeIn(() =>
        {
            DespawnPooledObjectAndTuto();
            m_gameData.ResetRun();
            UIManager.Instance.GetCanvas<SkillTreeCanvas>().Open();
        });

        GameData.Instance.runActive = false;
        OnRunEnd?.Invoke();
    }
    
    public void StartRun()
    {
        SetPause(false);
        GameData.Instance.runActive = true;
    
        UIManager.Instance.GetCanvas<SkillTreeCanvas>().Close();
        CameraController.Instance.SetControl(false);
    
        DespawnPooledObjectAndTuto();
    
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            CameraController.Instance.SetControl(true);
            OnRunStart?.Invoke();
        }
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