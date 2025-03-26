using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyLoginBox : BaseBox
{
    private static DailyLoginBox instance;
    public static DailyLoginBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<DailyLoginBox>(PathPrefabs.DAILY_LOGIN_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }

    [SerializeField] protected DailyLoginElement[] elements;

    public void Init()
    {
        for (int i = 0; i < elements.Length; i++)
        {
            int index = i;
            elements[i].Init(index);
        }
    }

    public void InitState()
    {
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].InitState();
        }
    }
}
