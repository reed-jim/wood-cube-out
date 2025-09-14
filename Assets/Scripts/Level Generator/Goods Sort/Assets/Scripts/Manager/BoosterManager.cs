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
            .Where(item => !item.ItemServiceLocator.itemProperty.IsClear)
            .GroupBy(i => i.TYPE)
            .Where(g => g.Count() >= 3)
            .Select(g => g.Take(3))
            .FirstOrDefault()
            .ToArray();

        SaferioTween.ScaleUpWithPunch(foodBox.transform, targetScale: 2, onCompleteAction: () =>
        {
            for (int i = 0; i < pickedItems.Length; i++)
            {
                int index = i;

                pickedItems[i].MoveToFoodBox(foodBox, onCompleteAction: () =>
                {
                    SaferioTween.ScaleDownWithPunch(foodBox.transform);
                });
            }

            GameplayUI.Instance.UpdateItemCountAfterMatch();
        });
    }
}
