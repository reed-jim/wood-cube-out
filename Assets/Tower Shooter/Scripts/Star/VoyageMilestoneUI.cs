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

    [Header("Shader Progress Fill")]
    public Image progressFill; // Image using the ProgressBar shader material
    private Material progressMaterial;

    private List<VoyageMilestoneItemUI> milestones = new List<VoyageMilestoneItemUI>();

    // Shader property IDs cached for performance
    private static readonly int ProgressProperty = Shader.PropertyToID("_Progress");
    private static readonly int MilestoneCountProperty = Shader.PropertyToID("_MilestoneCount");
    private static readonly int MilestonePercentsProperty = Shader.PropertyToID("_MilestonePercents");

    // Float array cache to avoid GC allocations
    private readonly float[] milestonePercentsBuffer = new float[16];

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        if (progressFill != null)
        {
            // Creates an instantiated material copy at runtime
            progressMaterial = progressFill.material;
        }
    }

    public void InitVoyageMilestones()
    {
        // 1. Recycle existing UI milestone overlays back to pool
        for (int i = 0; i < milestones.Count; i++)
        {
            VoyageMilestoneItemUI voyageMilestoneItemUI = milestones[i];
            ObjectPoolingEverything.ReturnToPool(GameConstants.voyageMilestoneItemUI, voyageMilestoneItemUI.gameObject);
        }
        milestones.Clear();

        VoyageMilestoneConfig voyageMilestoneConfig = StarManager.Ins.currentStarPathConfig.voyageMilestoneConfig;
        int count = Mathf.Min(voyageMilestoneConfig.voyageMilestones.Count, 16);

        // 2. Extract milestone percentages for the shader
        for (int i = 0; i < count; i++)
        {
            VoyageMilestone voyageMilestone = voyageMilestoneConfig.voyageMilestones[i];

            // Collect milestone percent position
            milestonePercentsBuffer[i] = voyageMilestone.percent;

            // Instantiates UI overlay item (rewards, checkmarks, click buttons)
            VoyageMilestoneItemUI voyageMilestoneItemUI = ObjectPoolingEverything.GetFromPool<VoyageMilestoneItemUI>(GameConstants.voyageMilestoneItemUI);
            voyageMilestoneItemUI.Init(voyageMilestone, container);
            milestones.Add(voyageMilestoneItemUI);
        }

        // 3. Send custom positions and milestone count to shader material
        if (progressMaterial != null)
        {
            progressMaterial.SetInt(MilestoneCountProperty, count);
            progressMaterial.SetFloatArray(MilestonePercentsProperty, milestonePercentsBuffer);
        }

        // 4. Update UI labels and planet visuals
        startLocationNameText.text = StarManager.Ins.currentStarPathConfig.starName;
        endLocationNameText.text = StarManager.Ins.nextStarPathConfig.starName;

        startLocationImage.material = StarManager.Ins.currentStarPathConfig.planetMaterial;
        endLocationImage.material = StarManager.Ins.nextStarPathConfig.planetMaterial;

        progressFill.transform.SetAsLastSibling();
    }

    public void SetVoyageProgress(float progress)
    {
        // Update shader progress bar visual smoothly
        if (progressMaterial != null)
        {
            progressMaterial.SetFloat(ProgressProperty, progress);
        }

        // Update reward and milestone triggers
        for (int i = 0; i < milestones.Count; i++)
        {
            VoyageMilestoneItemUI voyageMilestoneItemUI = milestones[i];
            voyageMilestoneItemUI.SetMilestoneProgress(progress);
        }
    }
}
