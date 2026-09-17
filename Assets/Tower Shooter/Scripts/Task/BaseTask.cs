using UnityEngine;

public class BaseTask : MonoBehaviour
{
    public float valueRequired;
    public float valueDone;

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

    public virtual void AddTaskValue(float value)
    {
        valueDone += value;
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
