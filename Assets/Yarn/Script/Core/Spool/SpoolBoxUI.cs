using System;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class SpoolBoxUI : MonoBehaviour
{
    [SerializeField] private SpoolBoxServiceLocator spoolBoxServiceLocator;
    [SerializeField] private SpriteRenderer screwBoxSpriteRenderer;
    [SerializeField] private RectTransform unlockByAdsButtonRT;
    [SerializeField] private RectTransform unlockBoxTextRT;
    [SerializeField] private Button unlockByAdsButton;
    [SerializeField] private SkeletonGraphic lockedBoxSkeleton;

    [Header("SPINE")]
    [SerializeField, SpineAnimation] string lockedBoxIdle;
    [SerializeField, SpineAnimation] string lockedBoxUnlock;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private ScrewBoxCameraObserver screwBoxCameraObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private Vector2Variable canvasSize;

    private Camera _screwBoxCamera;

    #region EVENT
    public static event Action<int> unlockSpoolBox;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    #endregion

    private void Awake()
    {
        ScrewBoxCameraManager.setCameraEvent += OnCameraSet;

        unlockByAdsButton.gameObject.SetActive(false);

        unlockByAdsButton.onClick.AddListener(ClickButtonUnlockByAds);

        SetupUI();
    }

    void OnDestroy()
    {
        ScrewBoxCameraManager.setCameraEvent -= OnCameraSet;
    }

    private async void SetupUI()
    {

    }

    private void OnCameraSet(Camera camera)
    {

    }

    public void SetUnlockByAdsButtonPosition()
    {
        unlockByAdsButtonRT.localPosition = Vector3.zero.ChangeZ(-4);
    }

    private void ClickButtonUnlockByAds()
    {
        showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockScrewBox);
    }

    public void Lock()
    {
        unlockByAdsButtonRT.localScale = Vector3.one;

        unlockByAdsButton.gameObject.SetActive(true);

        lockedBoxSkeleton.AnimationState.SetAnimation(0, lockedBoxIdle, false);
    }

    public void Unlock()
    {
        SpoolBox spoolBox = spoolBoxServiceLocator.spoolBox;

        spoolBox.IsInTransition = true;

        TrackEntry trackEntry = lockedBoxSkeleton.AnimationState.SetAnimation(0, lockedBoxUnlock, false);

        trackEntry.Complete += OnUnlockAnimationDone;

        // Tween.ShakeScale(transform, strength: 0.3f * Vector3.one, duration: 0.8f).OnComplete(() =>
        // {
        //     Tween.PositionY(unlockByAdsButtonRT, unlockByAdsButtonRT.position.y + 10, duration: 0.3f).OnComplete(() =>
        //     {
        //         unlockByAdsButton.gameObject.SetActive(false);

        //         spoolBox.IsInTransition = false;

        //         spoolBox.InvokeSpoolBoxUnlockedEvent();
        //     });
        // });

        unlockSpoolBox?.Invoke(gameObject.GetInstanceID());

        SoundManager.Instance.PlaySoundUnlockScrewBox();

        levelBoosterObserver.UseBooster(boosterIndex: GameConstants.UNLOCK_SCREW_BOX_BOOSTER_INDEX);

        void OnUnlockAnimationDone(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnUnlockAnimationDone;

            unlockByAdsButton.gameObject.SetActive(false);

            spoolBox.IsInTransition = false;

            spoolBox.InvokeSpoolBoxUnlockedEvent();
        }
    }

    public void HideUnlockBoxButton()
    {
        unlockByAdsButton.gameObject.SetActive(false);
    }
}
