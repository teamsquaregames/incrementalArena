using Sirenix.OdinInspector;
using Stats;
using TMPro;
using UnityEngine;
using Utils.UI;

public class RunTimerUIC : UIContainer
{
    [TitleGroup("Dependencies")]
    [Required] [SerializeField] private TMP_Text m_timeText;

    private float m_runDuration;
    private float m_timeRemaining;

    public float TimeRemaining => m_timeRemaining;
    public float RunDuration   => m_runDuration;

    public override void Init()
    {
        GameManager.Instance.onRunTimerStart += OnRunTimerStart;
        GameManager.Instance.onRunTimerEnd   += Close;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.onRunTimerStart -= OnRunTimerStart;
        GameManager.Instance.onRunTimerEnd   -= Close;
    }

    private void OnRunTimerStart()
    {
        float statValue = StatManager.Instance.GetDefinitionValue(EntityType.Player, StatType.RunDuration);
        m_runDuration   = statValue > 0f ? statValue : 60f;
        m_timeRemaining = m_runDuration;
        Open();
    }

    private void Update()
    {
        if (!m_isOpen || !GameData.Instance.runActive) return;

        if (GameConfig.Instance.cheatSettings.infiniteRunDuration)
        {
            m_timeText.text = "∞";
            return;
        }

        m_timeRemaining = Mathf.Max(0f, m_timeRemaining - Time.deltaTime);

        int totalSeconds = Mathf.CeilToInt(m_timeRemaining);
        Refresh(totalSeconds);

        if (m_timeRemaining <= 0f)
            GameManager.Instance.EndRun();
    }

    private void Refresh(int totalSeconds)
    {
        int minutes     = totalSeconds / 60;
        int seconds     = totalSeconds % 60;
        m_timeText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}
