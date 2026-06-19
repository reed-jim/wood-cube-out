using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelItemData
{
    public List<int> poolFactionIndexes;
}

public class Test
{
    public static void Main()
    {
        string json = ""{}"";
        LevelItemData data = JsonUtility.FromJson<LevelItemData>(json);
        Console.WriteLine(data.poolFactionIndexes == null);
    }
}
