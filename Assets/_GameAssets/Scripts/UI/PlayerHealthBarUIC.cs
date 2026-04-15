using UnityEngine;
using Utils.UI;

public class PlayerHealthBarUIC : UIContainer
{
    [SerializeField] private GenericGauge m_playerHealthBar;
    
    public GenericGauge PlayerHealthBar => m_playerHealthBar;
}
