using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TypeTutorial
{
    hard = 0,//Cứng
    soft = 1,//Mềm
    direct = 2,//
}
public abstract class TutorialBase : MonoBehaviour
{
    protected string nameTut;
    public TypeTutorial typeTut;
    protected TutorialFunController controller;
    [SerializeField] protected GameObject handTut;

    public virtual void Init(TutorialFunController controller)
    {
        this.controller = controller;
        SetNameTut();
    }

    protected abstract void SetNameTut();

    public virtual bool IsCanShowTut()
    {

        if (GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + nameTut, 0) != 0)
            return false;


        return true;
    }    

    public abstract void StartTut();

    public abstract bool IsCanEndTut();


    public virtual void OnEndTut()
    {
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + nameTut, 1);
    }

    protected virtual void OnUpdate()
    {

    }    

    private void Update()
    {
        OnUpdate();
    }
}
