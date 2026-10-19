using System.Collections.Generic;
using UnityEngine;

public class NodeData
{
    public Vector2Int coordinate;
    public bool isUnlocked;
    public float recycleModuleRate;

    public List<int> activeAugmentsIds;
}

public class NodeGroupData
{
    public List<NodeData> nodeDataItems;
}

public class GameData
{
    public bool isExist;
    public List<NodeGroupData> nodeGroupDataItems;
    public UnlockFeatureData unlockFeatureData;
}
