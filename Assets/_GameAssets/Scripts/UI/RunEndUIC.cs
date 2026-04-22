using TMPro;
using UnityEngine;
using Utils.UI;

public class RunEndUIC : UIContainer
{
    [SerializeField] private TextMeshProUGUI m_nbEnemiesKilledText;
    [SerializeField] private TextMeshProUGUI m_goldGainedText;
    
    public override void Open()
    {
        base.Open();
        
        double enemiesKilled = GameData.Instance.GetTrackedValue(TrackedValueType.EnemiesKilledThisRun);
        double goldGained = GameData.Instance.GetTrackedValue(TrackedValueType.CoinGainedThisRun);

        if (m_nbEnemiesKilledText != null)
            m_nbEnemiesKilledText.text = enemiesKilled.ToString("0");

        if (m_goldGainedText != null)
            m_goldGainedText.text = goldGained.ToString("0");
    }

    public void OnSkillTreeButtonClicked()
    {
        GameManager.Instance.EndRun();
    }

    public void OnNewRunButtonClicked()
    {
        GameManager.Instance.FadeAndEnterRun();
    }
}
