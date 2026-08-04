using UnityEngine;

public static class LayerManager
{
    public static float GetGrainZ()
    {
        return -10;
    }

    public static float GetBucketZ(int siblingIndex)
    {
        return -9 - 0.001f * siblingIndex;
    }
}
