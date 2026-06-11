using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionLimitQueue : MonoBehaviour
{

    public enum NameAction
    {
        None = 0,
        FxAntComeToNest = 0
    }    

    public class ActionLimit
    {
        public UnityAction action;
        public float spaceDelayTime;

        public float timmer;
        public bool isActiving;

        public ActionLimit(UnityAction action, float spaceDelayTime)
        {
            this.action = action;
            this.spaceDelayTime = spaceDelayTime;
        }

        public void CheckCallAction()
        {
            if(isActiving)
            {
                timmer -= Time.deltaTime;
                if(timmer < 0)
                {
                    timmer = 0;
                    isActiving = false;
                }    
            }    
        }
    }

    public Dictionary<NameAction, ActionLimit> Actions;
   
    public void Init()
    {
        Actions = new Dictionary<NameAction, ActionLimit>();
    }    

    public void AddAction(NameAction nameAction, ActionLimit act)
    {
        if (Actions == null)
            Actions = new Dictionary<NameAction,ActionLimit>();

        if(!Actions.ContainsKey(nameAction))
        {
            
            Actions.Add(nameAction, act);
        }    
    }    


    public void PlayAction(NameAction nameAction)
    {
        if (Actions.ContainsKey(nameAction))
        {
            if (Actions[nameAction].timmer <= 0)
            {
                Actions[nameAction].timmer = Actions[nameAction].spaceDelayTime;
                Actions[nameAction].isActiving = true;
                Actions[nameAction].action?.Invoke();
            }
        }
    }    

    // Update is called once per frame
    void Update()
    {
        foreach (var act in Actions)
        {
            act.Value.CheckCallAction();
        }    
    }


}
