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
        try
        {
            RemoteConfigLevelList remoteConfigLevelList;

            RemoteConfigController.GetJsonConfig(FirebaseConfig.REMOTE_CONFIG_LEVEL_LIST, out remoteConfigLevelList);

            if (remoteConfigLevelList.toReplaceLevelList == null)
            {
                return originalLevelIndex;
            }

            int index = -1;

            for (int i = 0; i < remoteConfigLevelList.toReplaceLevelList.Length; i++)
            {
                if (remoteConfigLevelList.toReplaceLevelList[i].key == originalLevelIndex)
                {
                    index = i;

                    break;
                }
            }

            if (index == -1)
            {
                return originalLevelIndex;
            }

            return remoteConfigLevelList.toReplaceLevelList[index].value;
        }
        catch (System.Exception)
        {

        }

        return originalLevelIndex;
    }

    public static int GetReplacedLevelIndexInGroup(int originalLevelIndex)
    {
        try
        {
            RemoteConfigLevelList remoteConfigLevelList;

            RemoteConfigController.GetJsonConfig(FirebaseConfig.REMOTE_CONFIG_LEVEL_LIST, out remoteConfigLevelList);

            if (remoteConfigLevelList.toReplaceLevelGroupList == null)
            {
                return originalLevelIndex;
            }

            for (int i = 0; i < remoteConfigLevelList.toReplaceLevelGroupList.Length; i++)
            {
                if (remoteConfigLevelList.toReplaceLevelGroupList[i].toReplaceGroup.Contains(originalLevelIndex))
                {
                    int index = System.Array.IndexOf(remoteConfigLevelList.toReplaceLevelGroupList[i].toReplaceGroup, originalLevelIndex);

                    return remoteConfigLevelList.toReplaceLevelGroupList[i].replaceWithGroup[index];
                }
            }

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
