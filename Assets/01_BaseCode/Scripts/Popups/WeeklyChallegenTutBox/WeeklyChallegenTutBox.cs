using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeeklyChallegenTutBox : BaseBox
{
    private static WeeklyChallegenTutBox instance;
    public static WeeklyChallegenTutBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<WeeklyChallegenTutBox>(PathPrefabs.WEEKLY_CHALLEGEN_TUT_BOX));
        }
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }
}
