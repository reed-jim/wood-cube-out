using TMPro;
using UnityEngine;

public class TaskItemUI : MonoBehaviour
{
    public TMP_Text taskName;
    public TMP_Text description;
    public TMP_Text progressText;

    private BaseTask task;

    public void Init(BaseTask task)
    {
        description.text = task.GetDescription();
        progressText.text = task.GetProgressText();

        this.task = task;
    }

    public void UpdateProgress()
    {
        progressText.text = task.GetProgressText();
    }
}
