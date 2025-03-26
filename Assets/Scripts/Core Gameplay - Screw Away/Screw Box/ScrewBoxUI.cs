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
    [SerializeField] private Text numItemTxt;
    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private ScrewBoxCameraObserver screwBoxCameraObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private Vector2Variable canvasSize;

    private Camera _screwBoxCamera;

    private void Awake()
    {
        unlockByAdsButton.gameObject.SetActive(false);
        unlockByAdsButton.onClick.AddListener(ClickButtonUnlockByAds);
        SetupUI();
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUYED_ITEM, o =>
        {
            SetupUI();
        } );
    }

    private void OnDisable()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUYED_ITEM, o =>
        {
            SetupUI();
        } );
    }

    private async void SetupUI()
    {
        if (UseProfile.GetNumItem(GiftType.AddBox) > 0)
        {
            numItemTxt.text = UseProfile.GetNumItem(GiftType.AddBox).ToString();
        }
        else numItemTxt.text = "+";
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
        int numItem = UseProfile.GetNumItem(GiftType.AddBox);
        if (numItem > 0)
        {
                Unlock();
                GameController.Instance.dataContain.giftDatabase.Claim(GiftType.AddBox, -1, Reason.in_campain_level);
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, GiftType.AddBox);

                if (BirdSort2.GamePlayController_BirdSort.Instance != null)
                {
                    BirdSort2.GamePlayController_BirdSort.Instance.tutItemInGameController.NextTut();
                }
          
            if (!BirdSort2.UseProfile.hadBuySuggest)
                BirdSort2.GamePlayController_BirdSort.Instance.suggetBuyItemController.OnUseItem(GiftType.AddBox);
        }
        else
        {
            //Mở shop
            if (BirdSort2.GamePlayController_BirdSort.Instance != null)
            {
                BirdSort2.GamePlayController_BirdSort.Instance.level.PauseLevel(true);
                var box = BuyItemBox.Setup();
                box.Show(GiftType.AddBox);

                box.OnCloseBox = () =>
                {
                    BirdSort2.GamePlayController_BirdSort.Instance.level.PauseLevel(false);
                };
            }
            else
            {
                BuyItemBox.Setup().Show(GiftType.AddBox);
            }
        }
        GameController.Instance.musicManager.PlayClickSound();
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

        Tween.ShakeScale(transform, strength: 0.3f * Vector3.one, duration: 1f).OnComplete(() =>
        {
            Tween.PositionY(unlockByAdsButtonRT, unlockByAdsButtonRT.position.y + 10, duration: 0.3f).OnComplete(() =>
            {
                unlockByAdsButton.gameObject.SetActive(false);

                screwBox.IsInTransition = false;

                screwBox.InvokeScrewBoxUnlockedEvent();
            });
        });

        gameObject.GetComponent<ScrewBox>().Unlock(gameObject.GetInstanceID());

        GameController.Instance.musicManager.PlayOneShot(BirdSort2.NameMusic.UnlockAdsScrewBox);
        
    }
}
