using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageBase : MonoBehaviour
{
    public PageName idPage;
    public ChoicePageUI choicePageUI;

    protected PageController controller;

    public virtual void Init(PageController controller)
    {
        this.controller = controller;
        choicePageUI.Init(this);

        InitState();
    }

    public virtual void InitState()
    {

    }

    public virtual void OnChoicePage()
    {
        InitState();
        choicePageUI.OnChoicePage();
    }

    public virtual void OnHidePage()
    {
        choicePageUI.OnHidePage();
    }    
}
