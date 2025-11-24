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
        showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockSpoolBox);
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

        Vector3 cachedScale = lockedBoxSkeleton.transform.localScale;

        Tween.Scale(lockedBoxSkeleton.transform, 1.1f * cachedScale, duration: 0.3f, ease: Ease.InOutSine).OnComplete(() =>
        {
            TrackEntry trackEntry = lockedBoxSkeleton.AnimationState.SetAnimation(0, lockedBoxUnlock, false);

            trackEntry.Complete += OnUnlockAnimationDone;

            unlockSpoolBox?.Invoke(gameObject.GetInstanceID());

            SoundManager.Instance.PlaySoundUnlockScrewBox();

            levelBoosterObserver.UseBooster(BoosterType.UnlockSpoolBox);
        });

        void OnUnlockAnimationDone(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnUnlockAnimationDone;

            lockedBoxSkeleton.transform.localScale = cachedScale;

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
