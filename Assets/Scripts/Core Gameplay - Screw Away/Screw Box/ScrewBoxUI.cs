using System;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class ScrewBoxUI : MonoBehaviour
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
    public static event Action<int> unlockScrewBox;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    #endregion

    private void Awake()
    {
        unlockByAdsButton.gameObject.SetActive(false);
        unlockByAdsButton.onClick.AddListener(ClickButtonUnlockByAds);
        SetupUI();
    }

   

    private async void SetupUI()
    {
        // await Task.Delay(500);

        // float orthoSize = _screwBoxCamera.orthographicSize;

        // float screenHeightWorld = orthoSize * 2;
        // float screenWidthWorld = screenHeightWorld * _screwBoxCamera.aspect;

        // float ratio = screwBoxSpriteRenderer.bounds.size.x / screenWidthWorld;

        // UIUtil.SetSizeKeepRatioY(unlockByAdsButtonRT, ratio * canvasSize.Value.x);
    }

    private void OnCameraSet(Camera camera)
    {
        // _screwBoxCamera = camera;

        // float orthoSize = _screwBoxCamera.orthographicSize;

        // float screenHeightWorld = orthoSize * 2;
        // float screenWidthWorld = screenHeightWorld * _screwBoxCamera.aspect;

        // float ratio = screwBoxSpriteRenderer.bounds.size.x / screenWidthWorld;

        // UIUtil.SetSizeKeepRatioY(unlockByAdsButtonRT, ratio * GamePersistentVariable.canvasSize.x);
        // UIUtil.SetSize(unlockBoxTextRT, 0.7f * unlockByAdsButtonRT.sizeDelta);
    }

    public void SetUnlockByAdsButtonPosition()
    {
        unlockByAdsButtonRT.localPosition = Vector3.zero.ChangeZ(-4);
        // unlockByAdsButtonRT.localPosition = _screwBoxCamera.WorldToScreenPoint(transform.position) - 0.5f * (Vector3)GamePersistentVariable.canvasSize;
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
        Tween.PositionY(unlockByAdsButtonRT, unlockByAdsButtonRT.position.y + 10, duration: 0.3f).OnComplete(() =>
        {
            unlockByAdsButton.gameObject.SetActive(false);
        });

        unlockScrewBox?.Invoke(gameObject.GetInstanceID());

        SoundManager.Instance.PlaySoundUnlockScrewBox();

        levelBoosterObserver.UseBooster(boosterIndex: GameConstants.UNLOCK_SCREW_BOX_BOOSTER_INDEX);
    }
}
