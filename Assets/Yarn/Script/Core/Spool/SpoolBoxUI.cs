using System;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class SpoolBoxUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer screwBoxSpriteRenderer;
    [SerializeField] private RectTransform unlockByAdsButtonRT;
    [SerializeField] private RectTransform unlockBoxTextRT;
    [SerializeField] private Button unlockByAdsButton;

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
    }

    public void Unlock()
    {
        ScrewBox screwBox = GetComponent<ScrewBox>();

        screwBox.IsInTransition = true;

        Tween.ShakeScale(transform, strength: 0.3f * Vector3.one, duration: 0.8f).OnComplete(() =>
        {
            Tween.PositionY(unlockByAdsButtonRT, unlockByAdsButtonRT.position.y + 10, duration: 0.3f).OnComplete(() =>
            {
                unlockByAdsButton.gameObject.SetActive(false);

                screwBox.IsInTransition = false;

                screwBox.InvokeScrewBoxUnlockedEvent();
            });
        });

        unlockSpoolBox?.Invoke(gameObject.GetInstanceID());

        SoundManager.Instance.PlaySoundUnlockScrewBox();

        levelBoosterObserver.UseBooster(boosterIndex: GameConstants.UNLOCK_SCREW_BOX_BOOSTER_INDEX);
    }

    public void HideUnlockBoxButton()
    {
        unlockByAdsButton.gameObject.SetActive(false);
    }
}
