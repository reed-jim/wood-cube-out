using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public enum TypeChest
{
    StarChest = 0,
    LevelChest = 1,
    DailyLoginChest = 2,
    ChestFreeRoyalPass = 3,
    ChestIAPRoyalPass = 4,
	CoinChest = 5,
    DiamodChest_1 = 6,
    DiamodChest_2 = 7, 
    DiamodChest_3 = 8,
    RaceSpaceRank_1 = 9,
    RaceSpaceRank_2 = 10,
    RaceSpaceRank_3 = 11,

    StoryChest = 12
}

[System.Serializable]
public class TransGruopReward
{
    public Transform[] gruopPos;
}

public class OpenChestBox : BaseBox
{
    //https://www.youtube.com/watch?v=uTDC0lrMv0s
    //4:16

    private const string APPEAR = "appear";
    private const string IDLE_PRE_OPEN = "idle_pre_open";
    private const string OPEN = "open";
    private const string IDLE_OPENED = "idle_opened";

    private static OpenChestBox instance;
    public static OpenChestBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<OpenChestBox>(PathPrefabs.OPEN_CHEST_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }

    [SerializeField] protected List<SkeletonGraphic> chestBody;
    protected TypeChest typeChest;
    [SerializeField] protected ItemShowBase rewardPrefab;
    [SerializeField] protected Transform parentReward;
    protected List<ItemShowBase> poolRewards;

    [SerializeField] private Transform posStartItem;
    [SerializeField] private List<TransGruopReward> gruopPosResults;

    [SerializeField] private CanvasGroup tabToOpenGruop;
    [SerializeField] private Button tabToOpenBtn;

    [SerializeField] private CanvasGroup tabToClaimGruop;
    [SerializeField] private Button tabToClaimBtn;
    [SerializeField] private Button btnWatchVideoToX2;
    private List<RewardRandom> rewards;

    [SerializeField] private AudioClip appearChestClip;
    [SerializeField] private AudioSource appearChestSource;

    [SerializeField] private AudioClip openChestClip;
    [SerializeField] private AudioSource openChestSource;

    public void Init()
    {
        poolRewards = new List<ItemShowBase>();

        tabToOpenBtn.onClick.AddListener(OnClickOpenChest);
        tabToClaimBtn.onClick.AddListener(OnClickTabToClaim);
        //btnWatchVideoToX2.onClick.AddListener(OnClickToClaimX2);
    }

    public void InitState()
    {
        for (int i = 0; i < chestBody.Count; i++)
        {
            chestBody[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < poolRewards.Count; i++)
        {
            poolRewards[i].gameObject.SetActive(false);
        }

        tabToOpenGruop.DOKill();
        tabToOpenGruop.alpha = 0;
        tabToOpenBtn.gameObject.SetActive(false);
        tabToClaimGruop.DOKill();
        tabToClaimGruop.alpha = 0;
        tabToClaimBtn.gameObject.SetActive(false);
    }

    public void Show(TypeChest typeChest, List<RewardRandom> rewards)
    {
        Show();
        this.rewards = rewards;
        this.typeChest = typeChest;
        chestBody[(int)typeChest].gameObject.SetActive(true);
        PlayAnim(APPEAR, false, () =>
        {
            OnChestAppearDone();
        });

        GameController.Instance.musicManager.PlayOneShot(appearChestClip, appearChestSource);
    }
    private void SetUpTween()
    {
        tabToOpenGruop.DOKill();
        tabToOpenGruop.alpha = 0;

        tabToClaimGruop.DOKill();
        tabToClaimGruop.alpha = 0;
    }    

    private void OnChestAppearDone()
    {
        SetUpTween();

        tabToOpenGruop.DOFade(1, 0.5f);
        tabToOpenBtn.gameObject.SetActive(true);
        PlayAnim(IDLE_PRE_OPEN, true);
    }

    private void OnClickOpenChest()
    {
        tabToOpenGruop.DOKill();
        tabToOpenGruop.alpha = 1;

        tabToClaimGruop.DOKill();
        tabToClaimGruop.alpha = 0;

        tabToOpenGruop.DOFade(0, 0.3f);
        tabToOpenBtn.gameObject.SetActive(false);
        PlayAnim(OPEN, false, ()=> 
        {
            PlayAnim(IDLE_OPENED, true, () => 
            { 

            });
        });
        StartCoroutine(ShowRewards(this.rewards));
        GameController.Instance.musicManager.PlayClickSound();
    }

    private IEnumerator ShowRewards(List<RewardRandom> rewards)
    {
        yield return new WaitForSeconds(1.28f);//Căn chỉnh theo frame Open chest anim
        int indexPos = rewards.Count - 1;
        GameController.Instance.musicManager.PlayOneShot(openChestClip, openChestSource);
        for (int i = 0; i < rewards.Count; i++)
        {
            //Appear
            int index = i;
            ItemShowBase it = GetItemReward();
            it.Init(rewards[index]);
            it.transform.position = posStartItem.position;
            it.body.alpha = 0;
            it.body.DOKill();
            it.body.DOFade(1, 0.3f);
            
            //Move
            UnityAction actionDone = () => { };
            if(index == rewards.Count - 1)
            {
                actionDone = () =>
                {
                    SetUpTween();

                    tabToClaimGruop.DOFade(1, 0.5f);
                    tabToClaimBtn.gameObject.SetActive(true);
                };
            }

            it.transform.DOJump(gruopPosResults[indexPos].gruopPos[index].position, 0.5f, 1, 0.3f).OnComplete(()=> 
            {
                actionDone?.Invoke();
            });
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void OnClickTabToClaim()
    {
        int indexPos = rewards.Count - 1;
        for (int i = 0; i < gruopPosResults[indexPos].gruopPos.Length; i++)
        {
            int index = i;
            GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
              gruopPosResults[indexPos].gruopPos[index].transform.position,
              rewards[index].typeItem,
              rewards[index].amount,
               Color.white,
               isUnlimitedItem: rewards[index].isUnlimited
              );
        }

        Close();
    }
    // private void OnClickToClaimX2()
    //{
    //    Close();
    //}

    public virtual void PlayAnim(string nameAnim, bool isLoop, UnityAction actionDone = null)
    {
        chestBody[(int)typeChest].AnimationState.ClearTracks();
        var track = chestBody[(int)typeChest].AnimationState.SetAnimation(0, nameAnim, isLoop);
        track.Complete += (s) => { actionDone?.Invoke(); };
    }

    private ItemShowBase GetItemReward()
    {
        for (int i = 0; i < poolRewards.Count; i++)
        {
            if (!poolRewards[i].gameObject.activeSelf)
            {
                poolRewards[i].gameObject.SetActive(true);
                return poolRewards[i];
            }
        }

        ItemShowBase item = Instantiate(rewardPrefab, parentReward);
        poolRewards.Add(item);
        return item;
    }
}
