using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RemoteConfigLevelManager
{
    // public static RemoteConfigLevelManager Instance;

    // private Dictionary<int, int> toReplaceLevelList;

    // private void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);

    //         return;
    //     }
    //     else
    //     {
    //         Instance = this;

    //         DontDestroyOnLoad(gameObject);
    //     }
    // }

    public static int GetReplacedLevelIndex(int originalLevelIndex)
    {
        return originalLevelIndex;
    }

    public static int GetReplacedLevelIndexInGroup(int originalLevelIndex)
    {
        try
        {
            return originalLevelIndex;
        }
        catch (System.Exception)
        {
            return originalLevelIndex;
        }
    }
}

[System.Serializable]
public class RemoteConfigLevelList
{
    public RemoteConfigLevelPair[] toReplaceLevelList;
    public RemoteConfigLevelGroupPair[] toReplaceLevelGroupList;
}

[System.Serializable]
public class RemoteConfigLevelPair
{
    public int key;
    public int value;
}

[System.Serializable]
public class RemoteConfigLevelGroupPair
{
    public int[] toReplaceGroup;
    public int[] replaceWithGroup;
}
