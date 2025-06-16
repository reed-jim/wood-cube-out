using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance;

    [SerializeField] private TMP_Text numItemsLeftText;

    private List<Tween> _tweens;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

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
}
