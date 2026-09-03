using System.Collections.Generic;
using UnityEngine;

public class NodeData
{
    public Vector2Int coordinate;
    public bool isUnlocked;

    public List<StatModifier> upgrades;
}

public class NodeGroupData
{
    public List<NodeData> nodeDataItems;
}

public class GameData
{
    public List<NodeGroupData> nodeGroupDataItems;
}
