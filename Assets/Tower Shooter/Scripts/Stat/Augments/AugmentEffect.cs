using UnityEngine;

public abstract class AugmentEffect : ScriptableObject
{
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

    /// <summary>
    /// Evaluates the template string. Override GetDescriptionTokens to inject custom parameters.
    /// </summary>
    public virtual string GetFormattedDescription(StatColorConfigSO config)
    {
        if (string.IsNullOrEmpty(descriptionTemplate)) return string.Empty;

        var tokens = GetDescriptionTokens();
        return StatTextParser.ParseTemplate(descriptionTemplate, config, tokens);
    }

    /// <summary>
    /// Override this in child classes to expose fields (like ratio, cooldown) to the text parser.
    /// </summary>
    protected virtual (string Key, object Value)[] GetDescriptionTokens()
    {
        return System.Array.Empty<(string, object)>();
    }
}
