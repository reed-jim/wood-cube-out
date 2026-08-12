using UnityEngine;

public static class LayerManager
{
    public static float GetGrainZ()
    {
        return -9;
    }

    public static float GetBucketZ(int order)
    {
        return -9 - 0.00001f;
    }

    public static float GetBucketTopZ(int order)
    {
        return -9 - 1.1f;
    }

    public static float GetBucketTextZ(int order)
    {
        return -9 - 1.2f;
    }

    public static float GetGhostChickenZ(int order)
    {
        return -10 - 0.0001f * order;
    }

    public static float GetTutorialLevelOneHandZOffset()
    {
        return -3;
    }
}
