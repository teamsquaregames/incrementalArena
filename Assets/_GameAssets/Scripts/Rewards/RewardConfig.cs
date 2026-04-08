using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardConfig", menuName = "Rewards/RewardConfig")]
public class RewardConfig : ScriptableObject
{
    [TitleGroup("Settings")]
    [SerializeField] private CurrencyAsset m_currencyAsset;

    public CurrencyAsset CurrencyAsset => m_currencyAsset;
}
