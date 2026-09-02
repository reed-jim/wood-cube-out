using System.Collections.Generic;
using UnityEngine;

public class NodeManager : Singleton<NodeManager>
{
    public NodeDetailUI nodeDetailUI;

    public List<BaseCharacter> nodes;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        nodes = new List<BaseCharacter>();
    }
}
