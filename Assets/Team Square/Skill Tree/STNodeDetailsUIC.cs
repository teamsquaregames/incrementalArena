using System.Text;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.UI;


public class STNodeDetailsUIC : UIContainer
{
    [TitleGroup("Dependencies")]
    [SerializeField, Required] private Image m_icon;
    [SerializeField, Required] private TMP_Text m_name;
    [SerializeField, Required] private TMP_Text m_description;
    [SerializeField, Required] private CurrencyCostUIE m_currencyCostUIE;
    [SerializeField, Required] private GameObject[] m_levelObjects;
    [SerializeField, Required] private GameObject[] m_enabledLevelObjects;
    
    private STNodeAsset m_currentAsset;

    public override void Open()
    {
        base.Open();
        GameData.Instance.onNodeLevelUp += LevelUp;
    }

    public override void Close()
    {
        base.Close();
        GameData.Instance.onNodeLevelUp -= LevelUp;
    }

    public void Setup(STNodeButton nodeButton)
    {
        m_currentAsset = nodeButton.LinkedNodeAsset;
        int level = GameData.Instance.GetNodeLevel(m_currentAsset.ID);

        /// Set new informations
        m_icon.sprite = m_currentAsset.Icon;
        m_name.text = m_currentAsset.DisplayName;
        m_description.text = BuildNodeDescription(m_currentAsset, level);

        if (level >= m_currentAsset.MaxLevel)
            m_currencyCostUIE.Hide();
        else
        {
            m_currencyCostUIE.Show();
            m_currencyCostUIE.SetCurrencyCost(m_currentAsset.Cost[0].currencyAsset, m_currentAsset.Cost[0].GetAmount(level));
        }

        transform.position = nodeButton.transform.position;
        HandleLevelDisplay();
        
        Open();
    }

    private void HandleLevelDisplay()
    {
        for (int i = 0; i < m_levelObjects.Length; i++)
            m_levelObjects[i].SetActive(i < m_currentAsset.MaxLevel);

        for (int i = 0; i < m_enabledLevelObjects.Length; i++)
            m_enabledLevelObjects[i].SetActive(i < GameData.Instance.GetNodeLevel(m_currentAsset.ID));
    }

    public void LevelUp(int level)
    {
        if (m_currentAsset == null) return;
        
        m_description.text = BuildNodeDescription(m_currentAsset, level);

        if (level >= m_currentAsset.MaxLevel)
            m_currencyCostUIE.Hide();
        else
        {
            m_currencyCostUIE.Show();
            m_currencyCostUIE.SetCurrencyCost(m_currencyCostUIE.CurrencyAsset, m_currentAsset.Cost[0].GetAmount(level));
        }

        HandleLevelDisplay();
    }

    private string BuildNodeDescription(STNodeAsset _asset, int _level)
    {
        StringBuilder description = new StringBuilder();

        // Add the asset's description at the beginning if it exists
        if (!string.IsNullOrEmpty(_asset.Description))
        {
            description.Append(_asset.Description);
        }

        // Determine if this is the first unlock (level 0 = not yet unlocked)
        bool isFirstUnlock = _level == 0;
        // Determine if this is the max level
        bool isMaxLevel = _level >= _asset.MaxLevel;

        if (description.Length == 0)
        {
            return "No effects";
        }

        return description.ToString();
    }
}