using UnityEngine;

public abstract class AugmentEffect : ScriptableObject
{
    public string effectName;
    public Color effectColor;

    [Header("UI Description")]
    [TextArea(2, 4)]
    [SerializeField] protected string descriptionTemplate;
    // Example in Inspector: "Gain {Damage} equal to {ratio:Percent} of total {Armor}."

    public abstract void ApplyEffect(CharacterStats target, object source);
    public abstract void RemoveEffect(CharacterStats target, object source);

    public virtual void ApplyEffectOnStatChanged(CharacterStats target)
    {

    }

    public virtual void ApplyEffectOnCycleCompleted(CharacterStats target)
    {

    }

    public virtual void ApplyEffectOnNodeStatModified(CharacterStats target)
    {

    }

    public virtual void SetAugment(CharacterStats target, Augment augment)
    {

    }

    #region Node Group
    public virtual void ApplyEffect(NodeGroup nodeGroup)
    {

    }

    public virtual void ApplyEffectOnNodeStatModified(NodeGroup nodeGroup)
    {

    }

    public virtual void ApplyEffectWhenAnyNodeInGroupCompletesCycle(NodeGroup nodeGroup, BaseCharacter node)
    {

    }

    public virtual void SetAugment(NodeGroup nodeGroup, Augment augment)
    {

    }
    #endregion

    public virtual void ApplyGlobalEffect()
    {

    }

    /// <summary>
    /// Evaluates the template string. Override GetDescriptionTokens to inject custom parameters.
    /// </summary>
    public virtual string GetFormattedDescription(StatColorConfigSO config)
    {
        if (string.IsNullOrEmpty(descriptionTemplate)) return string.Empty;

        var tokens = GetDescriptionTokens();

        string effectDescription = StatTextParser.ParseTemplate(descriptionTemplate, config, tokens);

        string effectColorHex = ColorUtil.ColorToHex(effectColor);

        string finalDescription = $"<color={effectColorHex}>{effectName}:</color> {effectDescription}";

        return finalDescription;
    }

    public virtual string GetFormattedDescriptionTooltip(StatColorConfigSO config)
    {
        return "";
    }

    /// <summary>
    /// Override this in child classes to expose fields (like ratio, cooldown) to the text parser.
    /// </summary>
    protected virtual (string Key, object Value)[] GetDescriptionTokens()
    {
        return System.Array.Empty<(string, object)>();
    }
}
