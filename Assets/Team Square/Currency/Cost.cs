using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct Cost
{
    public CurrencyAsset currencyAsset;
    [SerializeField] private double[] m_amount;
    
    public double GetAmount(int index)
    {
        if (index < 0) //Used to return 0 (free) in the case of free buildings
            return 0;
        
        if (m_amount == null || m_amount.Length == 0)
        {
            Debug.LogWarning($"Cost.GetAmount: m_amount is null or empty for currency {currencyAsset}");
            return 0;
        }
        if (index >= m_amount.Length)
            return m_amount[m_amount.Length - 1];
        return m_amount[index];
    }

    // Allow setting the amounts from editor scripts
    public void SetAmounts(double[] amounts)
    {
        m_amount = amounts;
    }

    public void SetAmountAt(int index, double value)
    {
        if (m_amount == null || index < 0) return;
        if (index >= m_amount.Length)
        {
            // expand array to fit
            double[] newArr = new double[index + 1];
            for (int i = 0; i < m_amount.Length; i++) newArr[i] = m_amount[i];
            m_amount = newArr;
        }
        m_amount[index] = value;
    }

    public void ResizeAmounts(int targetSize)
    {
        targetSize = Mathf.Max(0, targetSize);

        if (m_amount == null)
        {
            m_amount = new double[targetSize];
            return;
        }

        if (m_amount.Length == targetSize)
            return;

        double[] resized = new double[targetSize];
        Array.Copy(m_amount, resized, Mathf.Min(m_amount.Length, targetSize));
        m_amount = resized;
    }
}