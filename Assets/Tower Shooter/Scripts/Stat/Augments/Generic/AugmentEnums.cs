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
    Inverse,
    None
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

public enum AugmentValueSource
{
    FlatValue,
    SourceStat
}

public enum AugmentLevel
{
    Orbital,        // Around a planet
    Planetary,      // Planet-scale
    Stellar,        // Star-scale
    Galactic,       // Galaxy-scale
    Cosmic,         // Universe-scale
    Dimensional,    // Beyond normal space
    Transcendent    // Ultimate
}
