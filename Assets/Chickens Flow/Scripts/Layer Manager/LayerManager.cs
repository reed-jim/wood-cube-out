using UnityEngine;

public static class LayerManager
{
    public static float GetGrainZ()
    {
        return -9;
    }

    public static float GetBucketZ(int order)
    {
        return -9 - 0.00001f * order;
    }

    public static float GetBucketTopZ(int order)
    {
        return -9 - 0.1f * order;
    }

    public static float GetBucketTextZ(int order)
    {
        return -9 - 0.11f * order;
    }
}
