using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class VoyageMilestoneItemUI : MonoBehaviour
{
    public RectTransform container;
    public Image fill;
    public Image reward;
    public Button rewardButton;
    public Image tick;

    private float startProgressValue;
    private float endProgressValue;

    private VoyageMilestone voyageMilestone;

    private Tween loopScaleRewardTween;

    void Awake()
    {
        rewardButton.onClick.AddListener(EarnReward);
    }

    public void Init(VoyageMilestone voyageMilestone, RectTransform voyageUIContainer)
    {
        Vector2 position = Vector2.zero;

        container.SetParent(voyageUIContainer);
        container.localScale = Vector3.one;

        if (voyageMilestone.isStart || voyageMilestone.isEnd)
        {
            container.sizeDelta = 36 * Vector3.one;
            fill.rectTransform.sizeDelta = 36 * Vector3.one;
        }

        position.x = Mathf.Lerp(-0.5f * voyageUIContainer.sizeDelta.x, 0.5f * voyageUIContainer.sizeDelta.x, voyageMilestone.percent);

        container.localPosition = position;

        reward.gameObject.SetActive(false);
        tick.gameObject.SetActive(false);

        float progressPerOneUnit = 1 / voyageUIContainer.sizeDelta.x;

        startProgressValue = voyageMilestone.percent - progressPerOneUnit * (0.5f * container.sizeDelta.x);
        endProgressValue = voyageMilestone.percent + progressPerOneUnit * (0.5f * container.sizeDelta.x);

        this.voyageMilestone = voyageMilestone;
    }

    public void SetMilestoneProgress(float voyageProgress)
    {
        if (voyageProgress <= endProgressValue)
        {
            fill.fillAmount = Mathf.Lerp(0, 1, Mathf.InverseLerp(startProgressValue, endProgressValue, voyageProgress));
        }
        else if (!voyageMilestone.isStart && !voyageMilestone.isEnd)
        {
            if (!reward.gameObject.activeSelf && !tick.gameObject.activeSelf)
            {
                rewardButton.interactable = false;

                SaferioTween.ShowWithPunch(reward.transform, onCompleteAction: () =>
                {
                    rewardButton.interactable = true;

                    loopScaleRewardTween = Tween.Scale(reward.transform, 1.1f, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
                });
            }
        }
    }

    private void EarnReward()
    {
        for (int i = 0; i < voyageMilestone.rewards.Count; i++)
        {
            ResourceDataItem resourceDataItem = voyageMilestone.rewards[i];

            ResourceManager.Ins.ChangeResource(resourceDataItem.resourceType, resourceDataItem.value);
        }

        rewardButton.interactable = false;

        if (loopScaleRewardTween.isAlive)
        {
            loopScaleRewardTween.Stop();
        }

        SaferioTween.ScaleDownWithPunch(reward.transform);

        SaferioTween.ShowWithPunch(tick.transform);
    }
}
