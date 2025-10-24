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
    [SerializeField] protected Button unlockByAdsButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private ScrewBoxCameraObserver screwBoxCameraObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private Vector2Variable canvasSize;

    private Camera _screwBoxCamera;

    #region EVENT
    public static event Action<int> unlockScrewBox;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    #endregion

    protected virtual void Awake()
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

    public virtual void Lock()
    {
        unlockByAdsButtonRT.localScale = Vector3.one;

        unlockByAdsButton.gameObject.SetActive(true);
    }

    public virtual void Unlock()
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

        unlockScrewBox?.Invoke(gameObject.GetInstanceID());

        SoundManager.Instance.PlaySoundUnlockScrewBox();

        levelBoosterObserver.UseBooster(boosterIndex: GameConstants.UNLOCK_SCREW_BOX_BOOSTER_INDEX);
    }

    public void HideUnlockBoxButton()
    {
        unlockByAdsButton.gameObject.SetActive(false);
    }
}
