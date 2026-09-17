using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskItemUI : MonoBehaviour
{
    public TMP_Text description;
    public TMP_Text progressText;
    public Image doneTick;
    public Button receiveRewardButton;

    private BaseTask task;

    void Awake()
    {
        receiveRewardButton.onClick.AddListener(ReceiveReward);
    }

    public void Init(BaseTask task)
    {
        description.text = task.GetDescription();
        progressText.text = task.GetProgressText();

        receiveRewardButton.interactable = false;

        this.task = task;
    }

    public void UpdateProgress()
    {
        progressText.text = task.GetProgressText();

        if (task.IsTaskCompleted())
        {
            SaferioTween.ScaleUpWithPunch(doneTick.transform, onCompleteAction: () =>
            {
                receiveRewardButton.interactable = true;
            });
        }
    }

    private void ReceiveReward()
    {
        for (int i = 0; i < task.rewards.Count; i++)
        {
            ResourceDataItem reward = task.rewards[i];

            ResourceManager.Ins.ChangeResource(reward.resourceType, reward.value);
        }

        SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.taskItemUI, gameObject);
        });

        TaskManager.Ins.RemoveTask(task);
    }
}
