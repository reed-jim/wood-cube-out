using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AddTimeItem_BirdSort : ItemBase
{
    public int bonusTime = 30;
    private Tweener _textTween;

    [SerializeField] protected AudioClip addTimeClip;
    [SerializeField] protected AudioSource addTimeSource;

    public override void Init()
    {
        bonusTime = RemoteConfigController.GetIntConfig(FirebaseConfig.BONUS_TIME_ITEM, 30);
    }

    protected override bool ActiveItemHandle()
    {

        //Diễn FX
        var textTime = BirdSort2.GamePlayController_BirdSort.Instance.gameScene.timePlayLevelTxt;
        var itemFX = BirdSort2.GamePlayController_BirdSort.Instance.itemController.view.GetItemFX();
        itemFX.transform.position = Vector3.zero;
        itemFX.Show(
            GiftType.AddTimeItem_BirdSort,
            bonusTime,
            textTime.gameObject.transform.position,
            5,
            () =>
            {
                float currentTime = BirdSort2.GamePlayController_BirdSort.Instance.level.timePlayRequire;
                float endTime = currentTime + bonusTime;
                valueCache = (int)currentTime;
                textTime.transform.DOKill();
                textTime.transform.DOScale(1.1f, 0.3f).OnComplete(() =>
                {
                    textTime.transform.DOScale(1f, 0.3f);
                    BirdSort2.GamePlayController_BirdSort.Instance.level.timePlayRequire += bonusTime;
                    
                    //BirdSort2.GamePlayController_BirdSort.Instance.countDownController.HandleAddTime(new System.TimeSpan(0,0, bonusTime) );

                    //PlayTweenTimeText((int)endTime);

                });
                //Gọi FX
                //..
                GameController.Instance.musicManager.PlayOneShot(addTimeClip, addTimeSource);
            },
            isTime: true
            );

        return true;
    }

    int valueCache;
    private void PlayTweenTimeText(int targetValue)
    {
        _textTween = _textTween ??
                       DOTween.To(() => valueCache, value => {
                           valueCache = value;
                           GamePlayController.Instance.gameScene.timePlayLevelTxt.text = TimeManager.ShowTime2((long)value);
                       }, targetValue, 1f)
                              .SetUpdate(true)
                              .SetAutoKill(false).OnComplete(() => {
                                  GamePlayController.Instance.gameScene.timePlayLevelTxt.text = TimeManager.ShowTime2((long)targetValue);
                              });
        _textTween.ChangeStartValue(valueCache);
        _textTween.ChangeEndValue(targetValue);
        _textTween.Play();
    }

    public override void EndItem()
    {

    }

    public override bool IsCanUseItem()
    {
        return true;
    }
}
