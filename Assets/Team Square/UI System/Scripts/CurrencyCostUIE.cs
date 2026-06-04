using NUnit.Framework;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.UI;


public class CurrencyCostUIE : AUIElement
{
    [TitleGroup("Dependencies")]
    [SerializeField, Required] private TMP_Text m_costText;
    [SerializeField, Required] private Image m_icon;

    [SerializeField] private CurrencyAsset m_currency;
    public CurrencyAsset CurrencyAsset => m_currency;
    [SerializeField] private double m_cost;

    private GameData m_gameData => GameData.Instance;


    public override void Init()
    {
        m_gameData.onCurrencyChanged += OnCurrencyChanged;
    }

    public void SetCurrencyCost(CurrencyAsset _currency, double _cost)
    {
        // this.Log($"SetCurrencyCost: {_currency.name} - {_cost}");
        m_currency = _currency;
        m_icon.sprite = m_currency.Icon;
        m_cost = _cost;
        m_costText.text = NumberFormatter.FormatValue(m_cost);
        HasEnoughCurrencyFeedback();
    }

    private void OnCurrencyChanged(CurrencyAsset _currencyAsset, double _newValue)
    {
        if (_currencyAsset != m_currency)
            return;

        HasEnoughCurrencyFeedback();
    }

    private void HasEnoughCurrencyFeedback()
    {
        if (m_gameData.HasEnoughCurrency(m_currency, m_cost))
        {
            m_costText.color = Color.white;
        }
        else
        {
            m_costText.color = Color.red;
        }
    }
}