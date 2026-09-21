using UnityEngine;

public static class ScalingMath
{
    public static float Evaluate(float baseVal, float input, ScalingFunction func, float factor)
    {
        switch (func)
        {
            case ScalingFunction.Linear:
                return baseVal + (input * factor);

            case ScalingFunction.Exponential:
                return baseVal * Mathf.Pow(factor <= 0 ? 1.1f : factor, input);

            case ScalingFunction.Logarithmic:
                return baseVal + (Mathf.Log(Mathf.Max(1f, input)) * factor);

            case ScalingFunction.SquareRoot:
                return baseVal + (Mathf.Sqrt(Mathf.Max(0f, input)) * factor);

            case ScalingFunction.Inverse:
                return factor != 0 ? baseVal + (1f / (input * factor)) : baseVal;

            default:
                return input;
        }
    }

    public static float ApplyOperation(EffectOperation op, float targetVal, float calculatedVal)
    {
        switch (op)
        {
            case EffectOperation.Add:
                return targetVal + calculatedVal;

            case EffectOperation.Multiply:
                return targetVal * calculatedVal;

            case EffectOperation.SquareRoot:
                return Mathf.Sqrt(Mathf.Max(0f, calculatedVal));

            case EffectOperation.Logarithm:
                return Mathf.Log(Mathf.Max(1f, calculatedVal));

            case EffectOperation.Copy:
            case EffectOperation.Convert:
                return calculatedVal;

            default:
                return calculatedVal;
        }
    }
}
