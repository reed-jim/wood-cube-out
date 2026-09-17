using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            BaseTask task = ObjectPoolingEverything.GetFromPool<BaseTask>(GameConstants.baseTask);

            task.transform.SetParent(transform);
            task.transform.localScale = Vector3.one;

            (task as UpgradeNodeTask).Init(StatType.Damage, UnityEngine.Random.Range(3, 9));

            TaskItemUI taskItemUI = ObjectPoolingEverything.GetFromPool<TaskItemUI>(GameConstants.taskItemUI);

            CommonUtil.SetParentAndResetScale(taskItemUI.transform, taskContainer);

            taskItemUI.Init(task);

            addTaskItemUI?.Invoke(taskItemUI);
        }
    }

    private void ShowTaskPopup()
    {
        showTaskPopup?.Invoke();
    }
}
