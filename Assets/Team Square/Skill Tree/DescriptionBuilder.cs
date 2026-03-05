using System.Collections.Generic;
using System.Text;

public static class DescriptionBuilder
{
    /// <summary>
    /// Builds a complete description with custom description text, currencies, and stat modifiers
    /// </summary>
    public static string BuildFullDescription(
        string customDescription = null,
        IEnumerable<KeyValuePair<CurrencyAsset, double>> currencies = null,
        int statModifierLevel = 0)
    {
        StringBuilder description = new StringBuilder();

        // Add custom description at the beginning if it exists
        if (!string.IsNullOrEmpty(customDescription))
        {
            description.Append(customDescription);
        }

        // Add currencies
        if (currencies != null)
        {
            foreach (var currencyPair in currencies)
            {
                if (description.Length > 0)
                {
                    description.Append("\n");
                }

                string color = "#FFFF00";
                description.Append($"<color={color}>+{currencyPair.Value}</color> {currencyPair.Key.SpriteAssetString}");
            }
        }

        if (description.Length == 0)
        {
            return "No effects";
        }

        return description.ToString();
    }
    
    public static string FormatStatName(string statType)
    {
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < statType.Length; i++)
        {
            if (i > 0 && char.IsUpper(statType[i]))
            {
                result.Append(" ");
            }
            result.Append(statType[i]);
        }

        return result.ToString();
    }
}