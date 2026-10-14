using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using PrimeTween;

public class MilestoneFinishEffect : MonoBehaviour
{
    [Header("Arrow")]
    public CanvasGroup arrowCanvasGroup;
    public RectTransform arrow;
    private Vector3 arrowInitialPosition;
    private Tween arrowTween;

    public Button clickButton;

    [Header("Shockwave Effect")]
    [Tooltip("Assign the material using the URP_UIMilestoneShockwave shader")]
    public Material milestoneMaterial;
    [Tooltip("Target Image component that displays the shockwave material")]
    public Image shockwaveImage;
    public float burstDuration = 0.5f;

    private Material materialInstance;
    private Tween shockwaveTween;
    private bool isReached = false;

    private VoyageData voyageData;

    public static event Action<VoyageData> reachStarEvent;

    void Awake()
    {
        clickButton.onClick.AddListener(GoNextStar);
        arrowInitialPosition = arrow.localPosition;

        // Create a unique material instance so we don't modify the shared asset
        if (milestoneMaterial != null && shockwaveImage != null)
        {
            materialInstance = new Material(milestoneMaterial);
            shockwaveImage.material = materialInstance;
            materialInstance.SetFloat("_Progress", 0f);
        }
    }

    // Call this when the player arrives at the milestone location
    public void ReachMilestone(VoyageData voyageData)
    {
        if (isReached) return;
        isReached = true;

        gameObject.SetActive(true);

        PlayShockwaveAnimation(onComplete: PlayArrowAnimation);

        this.voyageData = voyageData;
    }

    private void GoNextStar()
    {
        gameObject.SetActive(false);

        if (arrowTween.isAlive)
        {
            arrowTween.Stop();
        }

        if (shockwaveTween.isAlive)
        {
            shockwaveTween.Stop();
        }

        reachStarEvent?.Invoke(voyageData);
    }

    #region Animations
    private void PlayArrowAnimation()
    {
        arrow.localPosition = arrowInitialPosition;
        arrowCanvasGroup.alpha = 0;
        clickButton.interactable = false;

        Tween.Alpha(arrowCanvasGroup, 1, duration: 0.3f).OnComplete(() =>
        {
            arrowTween = Tween.LocalPositionX(arrow, arrowInitialPosition.x - 40, duration: 1, cycles: -1, cycleMode: CycleMode.Yoyo);
            clickButton.interactable = true;
        });
    }

    private void PlayShockwaveAnimation(Action onComplete)
    {
        if (materialInstance == null) return;

        // Reset progress to 0
        materialInstance.SetFloat("_Progress", 0f);

        // Animate progress from 0 to 1 over the burst duration
        shockwaveTween = Tween.Custom(materialInstance, 0f, 1f, duration: burstDuration, (mat, val) =>
        {
            mat.SetFloat("_Progress", val);
        })
        .OnComplete(onComplete);
    }
    #endregion

    private void OnDestroy()
    {
        // Clean up the instantiated material to prevent memory leaks
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}