using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class CoinTopUI : BaseTopButtonGUI
{
    protected override int GetValue
    {
        get { return UseProfile.Coin; }
    }

    protected override void AddingEvent()
    {
        var _valueObservable = Observable.ReturnUnit()
                                         .Concat(GameUtils.AsObservable<GameEvent.OnIAPSucsses>().AsUnitObservable()
                                                          .Merge(GameUtils
                                                                 .AsObservable<GameEvent.HomeMessages.GoldMessage>()
                                                                 .AsUnitObservable()));
        _valueObservable.Subscribe(_ => OnUpdateValueEvent())
                        .AddTo(disposables);
        _valueObservable.ThrottleFirstFrame(1)
                        .Select(_ => GetValue)
                        .Buffer(2, 1)
                        .Subscribe(OnUpdateParticleEvent)
                        .AddTo(disposables);
    }

    protected override void OnClick()
    {
        //Open Shop
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.OPEN_SHOP);
        GameController.Instance.musicManager.PlayClickSound();
    }

    private void OnUpdateValueEvent()
    {
        UpdateValue();
    }

    private void OnUpdateParticleEvent(IList<int> messageList)
    {
        //if (messageList[1] > messageList[0] && receiveParticle != null)
        //    receiveParticle.Play();
    }
}
