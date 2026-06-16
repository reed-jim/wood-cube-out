using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeController : MonoBehaviour
{
    [SerializeField] private ThemeData themeData;
    [SerializeField] private Transform themeParent;
    public void Init()
    {
        int currentLevel = UseProfile.CurrentLevel;
        bool isHasTheme = false;
    }
}
