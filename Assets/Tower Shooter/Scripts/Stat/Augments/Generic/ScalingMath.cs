using System;
using UnityEngine;

public static class ScalingMath
{
    public static double Evaluate(float baseVal, double input, ScalingFunction func, float factor)
    {
        switch (func)
        {
            case ScalingFunction.Linear:
                return baseVal + (input * factor);

            case ScalingFunction.Exponential:
                return baseVal * Math.Pow(factor <= 0 ? 1.1f : factor, input);

            case ScalingFunction.Logarithmic:
                return baseVal + (Math.Log(Math.Max(1f, input)) * factor);

            case ScalingFunction.SquareRoot:
                return baseVal + (Math.Sqrt(Math.Max(0f, input)) * factor);

            case ScalingFunction.Inverse:
                return factor != 0 ? baseVal + (1f / (input * factor)) : baseVal;

            default:
                return input;
        }
    }

    public static double ApplyOperation(EffectOperation op, double targetVal, double calculatedVal)
    {
        switch (op)
        {
            case EffectOperation.Add:
                return targetVal + calculatedVal;

            case EffectOperation.Multiply:
                return targetVal * calculatedVal;

            case EffectOperation.SquareRoot:
                return Math.Sqrt(Math.Max(0, calculatedVal));

            case EffectOperation.Logarithm:
                return Math.Log(Math.Max(1f, calculatedVal));

            case EffectOperation.Copy:
            case EffectOperation.Convert:
                return calculatedVal;

            default:
                return calculatedVal;
        }
    }
}
