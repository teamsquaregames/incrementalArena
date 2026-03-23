using System;
using Sirenix.OdinInspector;
using TMPro;
using Utils.UI;

public class RunTimerUIC : UIContainer
{
    [TitleGroup("Dependencies")]
    [Required] [UnityEngine.SerializeField] private TMP_Text m_timeText;

    public override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        GameManager.Instance.OnRunStart += Open;
        GameManager.Instance.OnRunEnd   += Close;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnRunStart -= Open;
        GameManager.Instance.OnRunEnd   -= Close;
    }

    private void Update()
    {
        if (!m_isOpen || LevelManager.Instance == null) return;

        if (GameConfig.Instance.cheatSettings.infiniteRunDuration)
        {
            m_timeText.text = "∞";
            return;
        }

        int totalSeconds = UnityEngine.Mathf.CeilToInt(LevelManager.Instance.TimeRemaining);
        Refresh(totalSeconds);
    }

    private void Refresh(int totalSeconds)
    {
        int minutes     = totalSeconds / 60;
        int seconds     = totalSeconds % 60;
        m_timeText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}
