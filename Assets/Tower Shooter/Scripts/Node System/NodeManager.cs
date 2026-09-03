using System.Collections.Generic;
using UnityEngine;

public class NodeManager : Singleton<NodeManager>
{
    public NodeDetailUI nodeDetailUI;

    public List<BaseCharacter> nodes;

    public List<NodeGroup> nodeGroups;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        nodes = new List<BaseCharacter>();
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup nodeGroup = nodeGroups[i];

            nodeGroup.Init();
        }
    }
}
