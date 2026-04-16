using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils.UI;

public class EndRoundUIC : UIContainer
{
    [SerializeField] private Animator m_announceAnimator;
    [SerializeField] private TextMeshProUGUI m_announceText;


    public void SetAnnouncementText(string txt)
    {
        m_announceText.text = txt;
    }
    
    public override void Open()
    {
        base.Open();
        
        SetAnnouncementActive(true);
        
        DOVirtual.DelayedCall(1f, () =>
        {
            SetAnnouncementActive(false);
            DOVirtual.DelayedCall(0.5f, () => Close());
        });
    }

    private void SetAnnouncementActive(bool active)
    {
        m_announceAnimator.SetBool("Active", active);
    }
}
