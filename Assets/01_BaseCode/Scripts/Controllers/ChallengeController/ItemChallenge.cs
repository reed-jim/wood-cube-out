
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Spine.Unity;
using Sirenix.OdinInspector;
public class ItemChallenge : SerializedMonoBehaviour
{
    public const string APPEAR = "appear";
    public const string IDLE = "idle";
    public ChallengeType typeChallenge;
    [SerializeField] protected int LayerBase;
    ChallengeGameplayController challengeGameplayController;

    public Sprite icon;
    public SkeletonAnimation body;

    public  void Init( Transform posSpawn, UnityAction actionCome, bool isFlyOut, float delayTimeFlyOut = 0)
    {

        challengeGameplayController = BirdSort2.GamePlayController_BirdSort.Instance.challengeGameplayController;
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

    public  void Claimp(bool isUseManget = false)
    {
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EXIT_BIRD, this);
       body.gameObject.SetActive(false);
        int firstScore = challengeGameplayController.GetScoreItem(typeChallenge);
      //  BirdSort2.GamePlayController_BirdSort.Instance.itemShowFarmeController.SetUpFrame(typeChallenge);
        var itemFarme = BirdSort2.GamePlayController_BirdSort.Instance.itemShowFarmeController.GetFarme(typeChallenge);
        if (itemFarme != null)
        {
            itemFarme.Show(icon, firstScore);
        }

        int numItemAdd = UseProfile.IsUnlimitedItem(GiftType.X2WeeklyChallgeneItem) ? 2 : 1;

     
        //this.body.gameObject.SetActive(false);
        //Diễn FX

        var itemFX = BirdSort2.GamePlayController_BirdSort.Instance.itemController.view.GetItemFX();
        itemFX.transform.position = this.transform.position;
        challengeGameplayController.AddScoreItem(typeChallenge, numItemAdd);
        var currentScore = challengeGameplayController.GetScoreItem(typeChallenge);
        //Debug.Log("=========== itemFarme: " + itemFarme.iconItemImg.gameObject.transform.position);
        itemFX.Show(
            icon,
            numItemAdd,
            itemFarme.iconItemImg.gameObject.transform.position,
            Random.Range(0.5f,1.5f),
            () =>
            {
                if (itemFarme != null)
                {
                    itemFarme.UpdateScore(numItemAdd);
                }
                this.gameObject.SetActive(false);

            },
            isTime: false, destinationFollow: itemFarme.iconItemImg.transform, timeDelay: 0.05f, isJump: true, timeJump: 0.7f);


        if (GameController.Instance.useProfile.OnVibration)
        {
           // MMVibrationManager.Haptic(HapticTypes.Selection, false, true, this);
        }

        //BirdSort2.GamePlayController_BirdSort.Instance.tutWeeklyChallengeController.NextTut();
    }

    
}
