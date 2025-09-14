using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : Singleton<GameplayUI>
{
    [SerializeField] private TMP_Text numItemsLeftText;
    [SerializeField] private TMP_Text remainingTimeText;

    [Header("Booster")]
    [SerializeField] private Button moveFoodToBoxBoosterButton;
    [SerializeField] private Button shuffleBoosterButton;
    [SerializeField] private Button freezeBoosterButton;

    private List<Tween> _tweens;

    protected override void MoreActionOnAwake()
    {
        moveFoodToBoxBoosterButton.onClick.AddListener(BoosterManager.Instance.UseFoodBoxBooster);
        shuffleBoosterButton.onClick.AddListener(BoosterManager.Instance.UseShuffleBooster);
        freezeBoosterButton.onClick.AddListener(BoosterManager.Instance.UseFreezeTimeBooster);

        _tweens = new List<Tween>();
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    public void UpdateNumItems()
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Scale(numItemsLeftText.transform, 1.2f, duration: 0.15f, cycles: 2, cycleMode: CycleMode.Yoyo)
        .OnComplete(() =>
        {
            // numItemsLeftText.text = $"{GamePersistentVariable.numItemsLeft}";
        }));

        _tweens.Add(Tween.Custom(3, 0, duration: 0.3f, onValueChange: newVal =>
        {
            numItemsLeftText.text = $"{GamePersistentVariable.numItemsLeft + (int)newVal}";
        }));
    }

    public void UpdateItemCountAfterMatch()
    {
        GamePersistentVariable.numItemsLeft -= 3;

        UpdateNumItems();
    }

    public void UpdateRemainingTime(int remainingTime)
    {
        remainingTimeText.text = $"{CommonUtil.ToMMSS(remainingTime)}";
    }

    public void AnimateRemainingTimeText()
    {
        Tween.PunchScale(remainingTimeText.transform, strength: 0.1f * Vector3.one, duration: 0.3f);
    }
}
