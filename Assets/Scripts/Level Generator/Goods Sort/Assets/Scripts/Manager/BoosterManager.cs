using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BoosterManager : Singleton<BoosterManager>
{
    [SerializeField] private FoodBox foodBox;

    protected override void MoreActionOnAwake()
    {
        foodBox.gameObject.SetActive(false);
    }

    public void UseFoodBoxBooster()
    {
        BaseItem[] items = ItemManager.Instance.Items;

        BaseItem[] pickedItems = items
            .GroupBy(i => i.TYPE)
            .Where(g => g.Count() >= 3)
            .Select(g => g.Take(3))
            .FirstOrDefault()
            .ToArray();

        SaferioTween.ScaleUpWithPunch(foodBox.transform, targetScale: 1, onCompleteAction: () =>
        {
            for (int i = 0; i < pickedItems.Length; i++)
            {
                pickedItems[i].MoveToFoodBox(foodBox);
            }

            GameplayUI.Instance.UpdateItemCountAfterMatch();
        });
    }
}
