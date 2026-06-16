using DG.Tweening;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class EventSaleItem : SerializedMonoBehaviour
{
    public const string APPEAR = "appear";
    public const string IDLE = "idle";
    public EventSaleType typeEvent;
    [SerializeField] protected int LayerBase;
    [SerializeField] protected ExtralLayer fxIdleLayer;
    protected EventSaleInGameController eventSaleGameController;

    public Sprite icon;
    public SkeletonAnimation body;

    public void Init(Transform posSpawn, UnityAction actionCome, bool isFlyOut, float delayTimeFlyOut = 0)
    {

        eventSaleGameController = BirdSort2.GamePlayController_BirdSort.Instance.eventSaleController;
        this.transform.DOKill();
        this.transform.parent = posSpawn;
        this.transform.position = posSpawn.position;
        body.SetAnimation(APPEAR, false, delegate { body.SetAnimation(IDLE, true); });

        actionCome?.Invoke();
    }

    public void AppearItem(UnityAction actionItemReady)
    {
        //Anim Appear
        actionItemReady?.Invoke();
    }

    public void Claimp(bool isUseManget = false)
    {
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EXIT_BIRD, this);
        body.gameObject.SetActive(false);
        int firstScore = eventSaleGameController.GetScoreItem(typeEvent);
        var itemFarme = BirdSort2.GamePlayController_BirdSort.Instance.itemShowFarmeController.GetFarme(typeEvent);
        if (itemFarme != null)
        {
            itemFarme.Show(icon, firstScore);
        }

        var itemFX = BirdSort2.GamePlayController_BirdSort.Instance.itemController.view.GetItemFX();
        itemFX.transform.position = this.transform.position;
        eventSaleGameController.AddScoreItem(typeEvent, 1);
        var currentScore = eventSaleGameController.GetScoreItem(typeEvent);

        itemFX.Show(
            icon,
            1,
            itemFarme.iconItemImg.gameObject.transform.position,
            Random.Range(0.5f, 1.5f),
            () =>
            {
                if (itemFarme != null)
                {
                    itemFarme.UpdateScore(1);
                }
                this.gameObject.SetActive(false);

            },
            isTime: false, destinationFollow: itemFarme.iconItemImg.transform, timeDelay: 0.05f, isJump: true, timeJump: 0.7f);
    }


}
