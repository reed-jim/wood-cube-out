using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class LockedGrill : BaseShelf
{
    [SerializeField] private GameObject grillLidContainer;
    [SerializeField] private SpriteRenderer grillLid;
    [SerializeField] private SpriteRenderer toUnlockFoodIcon;

    [SerializeField] private ItemIconContainer itemIconContainer;

    private string _toUnlockType;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        clearItemsEvent += HandleItemCleared;
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();

        clearItemsEvent -= HandleItemCleared;
    }

    void Start()
    {
        Setup();
    }

    private void Setup()
    {
        Transform grillContainer = transform.parent;

        BaseItem[] items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(grillContainer).ToArray();

        string[] poolFactions = items.Select(item => item.TYPE).ToArray();

        _toUnlockType = poolFactions[Random.Range(0, poolFactions.Length)];

        Sprite toUnlockIcon = itemIconContainer.icons.First(item => item.name == _toUnlockType);

        toUnlockFoodIcon.sprite = toUnlockIcon;

        grillLidContainer.SetActive(true);
    }

    private void HandleItemCleared(BaseItem[] items)
    {
        if (items[0].TYPE == _toUnlockType)
        {
            Unlock();
        }
    }

    private void Unlock()
    {
        Tween.LocalPositionY(grillLidContainer.transform, grillLidContainer.transform.position.y + 3, duration: 0.3f);
        SaferioTween.ScaleDownWithPunch(grillLidContainer.transform, onCompleteAction: () =>
        {
            grillLidContainer.SetActive(false);
        });
    }
}
