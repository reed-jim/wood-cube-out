using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class MilestoneFinishEffect : MonoBehaviour
{
    public Button clickButton;

    [Tooltip("Assign the material using the URP_UIMilestoneClickPrompt shader")]
    public Material milestoneMaterial;
    public float burstDuration = 0.5f;

    private bool isReached = false;

    private VoyageData voyageData;

    public static event Action<VoyageData> reachStarEvent;

    void Awake()
    {
        clickButton.onClick.AddListener(GoNextStar);
    }

    // Call this when the player arrives at the milestone location
    public void ReachMilestone(VoyageData voyageData)
    {
        if (isReached) return;
        isReached = true;

        gameObject.SetActive(true);

        StartCoroutine(PlaySuccessBurstRoutine());

        this.voyageData = voyageData;
    }

    private IEnumerator PlaySuccessBurstRoutine()
    {
        float elapsed = 0f;
        milestoneMaterial.SetFloat("_UnlockProgress", 0.01f); // Kick off animation state

        while (elapsed < burstDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / burstDuration);
            milestoneMaterial.SetFloat("_UnlockProgress", progress);
            yield return null;
        }

        // Lock at 1.0. This permanently hands execution over to the repeating click-prompt idle wave in the shader.
        milestoneMaterial.SetFloat("_UnlockProgress", 1.0f);
    }

    private void GoNextStar()
    {
        reachStarEvent?.Invoke(voyageData);
    }
}
