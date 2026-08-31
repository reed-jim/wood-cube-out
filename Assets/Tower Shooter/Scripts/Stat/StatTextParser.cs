using System.Text.RegularExpressions;
using UnityEngine;

public static class StatTextParser
{
    // Regex matching tokens like {Damage}, {Armor}, {0:Percent}, {1:Value}
    private static readonly Regex TokenRegex = new Regex(@"\{(?<key>[^:]+)(:(?<format>[^}]+))?\}", RegexCompiledOption());

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
    /// Example input: "Gain {0:Percent} of total {Armor} as {Damage}."
    /// </summary>
    public static string ParseTemplate(string template, StatColorConfigSO config, params (string Key, object Value)[] customTokens)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        return TokenRegex.Replace(template, match =>
        {
            string key = match.Groups["key"].Value;
            string format = match.Groups["format"].Value;

            // 1. Check if token matches a StatType enum (e.g., {Damage}, {Armor})
            if (System.Enum.TryParse<StatType>(key, true, out var statType))
            {
                string hex = config.GetHex(statType);
                return $"<color=#{hex}>{statType}</color>";
            }

            // 2. Check custom tokens passed by caller
            for (int i = 0; i < customTokens.Length; i++)
            {
                if (customTokens[i].Key.Equals(key, System.StringComparison.OrdinalIgnoreCase))
                {
                    return FormatValue(customTokens[i].Value, format, config);
                }
            }

            return match.Value; // Return raw token if unmatched
        });
    }

    private static string FormatValue(object value, string format, StatColorConfigSO config)
    {
        if (value is float floatVal)
        {
            string valStr = format.ToLower() switch
            {
                "percent" => $"{floatVal * 100f}%",
                "flat" => floatVal >= 0 ? $"+{floatVal}" : floatVal.ToString(),
                _ => floatVal.ToString()
            };

            string hex = config.GetValueHex();
            return $"<color=#{hex}>{valStr}</color>";
        }

        return value?.ToString() ?? string.Empty;
    }
}
