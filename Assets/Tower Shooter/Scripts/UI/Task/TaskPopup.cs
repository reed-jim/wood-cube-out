using System.Collections.Generic;
using UnityEngine;

public class TaskPopup : BasePopup
{
    private List<TaskItemUI> taskItemUIs = new List<TaskItemUI>();

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        TaskManager.addTaskItemUI += AddTaskItem;
        TaskManager.showTaskPopup += ShowTaskPopup;
    }

    void OnDestroy()
    {
        TaskManager.addTaskItemUI -= AddTaskItem;
        TaskManager.showTaskPopup -= ShowTaskPopup;
    }

    private void ShowTaskPopup()
    {
        Show();

        UpdateAllTaskProgresses();
    }

    private void AddTaskItem(TaskItemUI taskItemUI)
    {
        taskItemUIs.Add(taskItemUI);
    }

    private void UpdateAllTaskProgresses()
    {
        for (int i = 0; i < taskItemUIs.Count; i++)
        {
            TaskItemUI taskItemUI = taskItemUIs[i];

            taskItemUI.UpdateProgress();
        }
    }
}
