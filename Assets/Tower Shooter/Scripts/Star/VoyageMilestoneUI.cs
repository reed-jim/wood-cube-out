using UnityEngine;
using UnityEngine.UI;

public class VoyageMilestoneUI : Singleton<VoyageMilestoneUI>
{
    public RectTransform container;
    public Image progressFill;

    public void InitVoyageMilestones()
    {
        for (int i = 0; i < StarManager.Ins.currentStarPathConfig.voyageMilestoneConfig.voyageMilestones.Count; i++)
        {
            VoyageMilestone voyageMilestone = StarManager.Ins.currentStarPathConfig.voyageMilestoneConfig.voyageMilestones[i];

            VoyageMilestoneItemUI voyageMilestoneItemUI = ObjectPoolingEverything.GetFromPool<VoyageMilestoneItemUI>(GameConstants.voyageMilestoneItemUI);

            voyageMilestoneItemUI.Init(voyageMilestone, container);
        }
    }

    public void SetVoyageProgress(float progress)
    {
        progressFill.fillAmount = progress;
    }
}
