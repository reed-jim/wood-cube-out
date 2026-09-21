public enum EffectTrigger
{
    OnCycleComplete,
    OnCrit,
    EveryXCycles,
    Permanently,
    WhenIdle,
    WhenStatCrossesThreshold
}

public enum EffectOperation
{
    Add,
    Multiply,
    Convert,
    Swap,
    SquareRoot,
    Logarithm,
    Copy
}

public enum ScalingFunction
{
    Linear,
    Exponential,
    Logarithmic,
    SquareRoot,
    Inverse
}

public enum ConditionType
{
    AlwaysActive,
    StatGreaterThanOther,
    StatLessThanOther,
    StatThresholdGate
}

public enum EffectScope
{
    Self,
    AdjacentNodes,
    Global
}

public enum EffectDuration
{
    Permanent,
    TemporaryBurst,
    Stacking,
    Decaying
}
