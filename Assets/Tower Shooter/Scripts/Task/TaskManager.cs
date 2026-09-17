using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TaskType
{
    UpgradeStatNode
}

public class TaskManager : Singleton<TaskManager>
{
    private List<BaseTask> tasks = new List<BaseTask>();

    public RectTransform taskContainer;
    public Button showTaskPopupButton;

    public static event Action<TaskItemUI> addTaskItemUI;
    public static event Action showTaskPopup;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        showTaskPopupButton.onClick.AddListener(ShowTaskPopup);
    }

    void Start()
    {
        AssignTask();
    }

    private void AssignTask()
    {
        int numTask = 1;

        for (int i = 0; i < numTask; i++)
        {
            BaseTask task = GetTaskComponentFromPool(TaskType.UpgradeStatNode);

            task.transform.SetParent(transform);
            task.transform.localScale = Vector3.one;

            (task as UpgradeNodeTask).Init(StatType.Damage, UnityEngine.Random.Range(3, 9));
            task.BuildRewards();

            TaskItemUI taskItemUI = ObjectPoolingEverything.GetFromPool<TaskItemUI>(GameConstants.taskItemUI);

            CommonUtil.SetParentAndResetScale(taskItemUI.transform, taskContainer);

            taskItemUI.Init(task);

            tasks.Add(task);

            addTaskItemUI?.Invoke(taskItemUI);
        }

        MultiverseIdleGameplayScreen.Ins.NotifyTask();
    }

    private BaseTask GetTaskComponentFromPool(TaskType taskType)
    {
        if (taskType == TaskType.UpgradeStatNode)
        {
            return ObjectPoolingEverything.GetFromPool<UpgradeNodeTask>(GameConstants.upgradeNodeTask);
        }

        return ObjectPoolingEverything.GetFromPool<BaseTask>(GameConstants.baseTask);
    }

    private void ShowTaskPopup()
    {
        showTaskPopup?.Invoke();

        MultiverseIdleGameplayScreen.Ins.HideTaskNotification();
    }

    public void RemoveTask(BaseTask task)
    {
        if (tasks.Contains(task))
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.baseTask, task.gameObject);

            tasks.Remove(task);
        }
    }
}
