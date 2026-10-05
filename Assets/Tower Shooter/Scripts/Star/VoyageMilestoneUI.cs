using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoyageMilestoneUI : Singleton<VoyageMilestoneUI>
{
    public RectTransform container;
    public TMP_Text startLocationNameText;
    public TMP_Text endLocationNameText;
    public Image startLocationImage;
    public Image endLocationImage;
    public Image progressFill;

    private List<VoyageMilestoneItemUI> milestones = new List<VoyageMilestoneItemUI>();

    public void InitVoyageMilestones()
    {
        for (int i = 0; i < milestones.Count; i++)
        {
            VoyageMilestoneItemUI voyageMilestoneItemUI = milestones[i];

            ObjectPoolingEverything.ReturnToPool(GameConstants.voyageMilestoneItemUI, voyageMilestoneItemUI.gameObject);
        }

        milestones.Clear();

        VoyageMilestoneConfig voyageMilestoneConfig = StarManager.Ins.currentStarPathConfig.voyageMilestoneConfig;

        for (int i = 0; i < voyageMilestoneConfig.voyageMilestones.Count; i++)
        {
            VoyageMilestone voyageMilestone = voyageMilestoneConfig.voyageMilestones[i];

            VoyageMilestoneItemUI voyageMilestoneItemUI = ObjectPoolingEverything.GetFromPool<VoyageMilestoneItemUI>(GameConstants.voyageMilestoneItemUI);

            voyageMilestoneItemUI.Init(voyageMilestone, container);

            milestones.Add(voyageMilestoneItemUI);
        }

        startLocationNameText.text = voyageMilestoneConfig.startLocation;
        endLocationNameText.text = voyageMilestoneConfig.endLocation;

        startLocationImage.sprite = StarManager.Ins.currentStarPathConfig.picture;
        endLocationImage.sprite = StarManager.Ins.nextStarPathConfig.picture;

        progressFill.transform.SetAsLastSibling();
    }

    public void SetVoyageProgress(float progress)
    {
        progressFill.fillAmount = progress;

        for (int i = 0; i < milestones.Count; i++)
        {
            VoyageMilestoneItemUI voyageMilestoneItemUI = milestones[i];

            voyageMilestoneItemUI.SetMilestoneProgress(progress);
        }
    }
}
