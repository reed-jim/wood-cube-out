using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeDataContainer", menuName = "Idle/UI/ThemeDataContainer")]
public class ThemeDataContainer : ScriptableObject
{
    public List<ThemeConfig> items;
}
