using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ThemeData", fileName = "ThemeData.asset")]
public class ThemeData : ScriptableObject
{
    public List<ThemeDataModle> data;
}

[System.Serializable]
public class ThemeDataModle
{
    public int startLevel;
    public int endLevel;
    public GameObject themeObj;
}
