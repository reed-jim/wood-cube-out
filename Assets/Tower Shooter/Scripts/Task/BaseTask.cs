using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseTask : MonoBehaviour
{
    public TaskType taskType;
    public float valueRequired;
    public float valueDone;

    public List<ResourceDataItem> rewards;

    public static event Action taskCompleteEvent;

    private void Awake()
    {
        MoreActionOnAwake();
    }

    private void OnDestroy()
    {
        MoreActionOnDestroy();
    }

    protected virtual void MoreActionOnAwake()
    {

    }

    protected virtual void MoreActionOnDestroy()
    {

    }

    public void BuildRewards()
    {
        rewards = new List<ResourceDataItem>
        {
            new ResourceDataItem(ResourceType.Coin, 500)
        };
    }

    public virtual void AddTaskValue(float value)
    {
        valueDone += value;

        CheckTaskComplete();
    }

    public virtual void CheckTaskComplete()
    {
        if (IsTaskCompleted())
        {
            taskCompleteEvent?.Invoke();
        }
    }

    public virtual bool IsTaskCompleted()
    {
        return false;
    }

    public virtual string GetDescription()
    {
        return "";
    }

    public virtual string GetProgressText()
    {
        return $"{valueDone}/{valueRequired}";
    }
}
