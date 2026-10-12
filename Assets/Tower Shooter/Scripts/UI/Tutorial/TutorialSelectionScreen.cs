using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSelectionScreen : SwitchableScreen
{
    [SerializeField] private ObjectPoolingPageView pageView;

    [SerializeField] private TutorialItemData[] tutorialItemDataArray;

    protected override void Show()
    {
        base.Show();

        Init();
    }

    private void Init()
    {
        pageView.Init();
    }

    public TutorialItemData GetTutorialItemData(int index)
    {
        if (index >= 0 && index < tutorialItemDataArray.Length)
        {
            return tutorialItemDataArray[index];
        }
        else
        {
            return null;
        }
    }
}

[Serializable]
public class TutorialItemData
{
    public int id;
    public string tutorialName;
    public BaseTutorial tutorialComponent;
}
