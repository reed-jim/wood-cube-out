using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFunController : MonoBehaviour
{
    [SerializeField] protected List<TutorialBase> tutorials;
    protected int currentIDTut;

    public virtual void Init()
    {
        currentIDTut = 0;
        for (int i = 0; i < tutorials.Count; i++)
        {
            tutorials[i].Init(this);
        }
    }

    public virtual void OnBirdReady()
    {

    }

    public void StartTut()
    {
        for (int i = 0; i < tutorials.Count; i++)
        {     
            if (tutorials[i].IsCanShowTut())
            {
                tutorials[i].StartTut();
                currentIDTut = i;
                break;
            }
        }
    


    }

      

    public void NextTut()
    {
        if (currentIDTut >= tutorials.Count)
            return;

        if (!tutorials[currentIDTut].IsCanShowTut())
            return;

        if (!tutorials[currentIDTut].IsCanEndTut())
            return;

        tutorials[currentIDTut].OnEndTut();
        currentIDTut++;
        if (currentIDTut >= tutorials.Count)
            return;

        if (tutorials[currentIDTut].IsCanShowTut())
        {
            tutorials[currentIDTut].StartTut();
        }
    }    
}
