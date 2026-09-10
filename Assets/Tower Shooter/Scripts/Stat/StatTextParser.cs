using System.Text.RegularExpressions;
using UnityEngine;

public static class StatTextParser
{
    // Regex matching tokens like {Damage}, {Armor}, {0:Percent}, {1:Value}
    private static readonly Regex TokenRegex =
    new Regex(
        @"\{(?<key>[^:{}]+)(:(?<format>[^}]+))?\}",
        RegexCompiledOption()
    );

    private static RegexOptions RegexCompiledOption()
    {
#if UNITY_EDITOR
        return RegexOptions.None; // Faster compile times in Editor
#else
        return RegexOptions.Compiled;
#endif
    }

    /// <summary>
    /// Replaces tokens in template text with colored values.
    /// Example input: "Neighbor nodes get {value0:percent:AttackSpeed} {AttackSpeed}."
    /// </summary>
    public static string ParseTemplate(string template, StatColorConfigSO config, params (string Key, object Value)[] customTokens)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        return TokenRegex.Replace(template, match =>
        {
            string key = match.Groups["key"].Value;
            string formatSpec = match.Groups["format"].Value; // e.g., "percent:AttackSpeed" or "percent"

            // 1. Direct Stat Token check (e.g., {AttackSpeed})
            if (System.Enum.TryParse<StatType>(key, true, out var statType))
            {
                string hex = config.GetHex(statType);
                string displayText = GetStatDisplayText(statType, hex); // Shortened text mapping
                return $"<color=#{hex}>{displayText}</color>";
            }

            // 2. Custom Token check (e.g., {value0:percent:AttackSpeed})
            for (int i = 0; i < customTokens.Length; i++)
            {
                if (customTokens[i].Key.Equals(key, System.StringComparison.OrdinalIgnoreCase))
                {
                    return FormatValue(customTokens[i].Value, formatSpec, config);
                }
            }

            return match.Value; // Fallback for unmatched tokens
        });
    }

    private static string FormatValue(object value, string formatSpec, StatColorConfigSO config)
    {
        // Split "percent:AttackSpeed" into format ("percent") and stat ("AttackSpeed")
        string[] parts = formatSpec.Split(':');
        string formatType = parts[0].ToLower();

        // Default to standard value color
        string hex = config.GetValueHex();

        // Check if a linked stat type was provided after the second colon
        if (parts.Length > 1 && System.Enum.TryParse<StatType>(parts[1], true, out var linkedStat))
        {
            hex = config.GetHex(linkedStat);
        }

        // Format value
        string valStr = value?.ToString() ?? string.Empty;
        if (value is float floatVal)
        {
            valStr = formatType switch
            {
                "percent" => $"{floatVal * 100f}%",
                "flat" => floatVal >= 0 ? $"+{floatVal}" : floatVal.ToString(),
                _ => floatVal.ToString()
            };
        }

        return $"<color=#{hex}>{valStr}</color>";
    }

    // Display Name Mapping
    private static string GetStatDisplayText(StatType stat, string hex)
    {
        // Fetch the clean name or custom short string
        string shortName = stat switch
        {
            StatType.Damage => "Power",
            StatType.AttackSpeed => "Speed",
            _ => stat.ToString()
        };

        // Embed TMP's <sprite name="..."> tag using the enum string or custom name
        // Example output: <sprite name="AttackSpeed"> Atk Spd
        return $"{shortName}";
        return $"<voffset=16><sprite name=\"{stat}\" color=#{hex}></voffset>{shortName}";
    }
}
