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


        for (int i = 0; i < themeData.data.Count; i++)
        {
            if (currentLevel >= themeData.data[i].startLevel && currentLevel <= themeData.data[i].endLevel)
            {
                Instantiate(themeData.data[i].themeObj, themeParent);
                isHasTheme = true;
                return;
            }
        }

        if (!isHasTheme)
        {
            Instantiate(themeData.data[Random.Range(0, themeData.data.Count)].themeObj, themeParent);
        }
    }
}
