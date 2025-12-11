using System;
using System.Threading.Tasks;
using NamPhuThuy.AnimateWithScripts;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class SpoolBoxUI : MonoBehaviour
{
    [Header("Flags")] 
    [SerializeField] private bool isTutorial;
    
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

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        ScrewBoxCameraManager.setCameraEvent += OnCameraSet;
        YarnLevelLoader.isTutAvailableEvent += CheckTutAvailable;

        unlockByAdsButton.gameObject.SetActive(false);

        unlockByAdsButton.onClick.AddListener(ClickButtonUnlockByAds);

        SetupUI();
    }

    void OnDestroy()
    {
        ScrewBoxCameraManager.setCameraEvent -= OnCameraSet;
        YarnLevelLoader.isTutAvailableEvent -= CheckTutAvailable;
        
        unlockByAdsButton.onClick.RemoveAllListeners();
    }

    #endregion

    #region Events Listen

    private void CheckTutAvailable(bool isTutAvailable)
    {
        this.isTutorial = isTutAvailable;
    }

    #endregion

    #region Private Methods

    private void PlayVFXPopupText(string message)
    {
        var args = new PopupTextArgs
        {
            message = Lean.Localization.LeanLocalization.GetTranslationText(message),
            // startParent = transform,
            textFont = GUIManager.Instance.DefaultFont,
            textColor = Color.white,
            duration = 1f,
            OnComplete = null,
        };
        AnimationManager.Ins.Play<PopupTextArgs>(args);
    }

    #endregion

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
        if (isTutorial)
        {
            PlayVFXPopupText(LocalizationConst.CANT_CLICK);
            return;
        }
        
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
