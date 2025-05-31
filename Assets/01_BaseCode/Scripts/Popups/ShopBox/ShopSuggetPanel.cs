using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopSuggetPanel : ShopPanel
{
    [SerializeField] private Button moreOffer;

    public override void Init()
    {
        base.Init();
        moreOffer.onClick.AddListener(OnClickMoreOffer);
    }
//he he he 
    public override void InitState()
    {
        base.InitState();
        moreOffer.gameObject.SetActive(true);
        var iapController = GameController.Instance.iapController;

        foreach (var item in saleItems)
        {
            var data = iapController.inappDatabase.GetPack(item.Key);
            if (!data.isHasInShopSugget)
                item.Value.gameObject.SetActive(false);
        }
        foreach (var item in salePriorityLowItems)
        {
            var data = iapController.inappDatabase.GetPack(item.Key);
            if (!data.isHasInShopSugget)
                item.Value.gameObject.SetActive(false);
        }

        for (int i = 0; i < bundlePacks.Length; i++)
        {
            var data = iapController.inappDatabase.GetPack(bundlePacks[i].typePack);
            if (!data.isHasInShopSugget)
                bundlePacks[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < coinPacks.Length; i++)
        {
            var data = iapController.inappDatabase.GetPack(coinPacks[i].typePack);
            if (!data.isHasInShopSugget)
                coinPacks[i].gameObject.SetActive(false);
        }

        //if (gameObject.activeInHierarchy)
        {
            if (actionReloadScroll != null)
                StopCoroutine(actionReloadScroll);

            actionReloadScroll = ReloadScroll();
            StartCoroutine(actionReloadScroll);
        }
    }

    private void OnClickMoreOffer()
    {
        base.InitState();
        moreOffer.gameObject.SetActive(false);

        // if (!isInited)
        {
            if (actionReloadScroll != null)
                StopCoroutine(actionReloadScroll);

            actionReloadScroll = ReloadScroll();
            StartCoroutine(actionReloadScroll);
        }
        GameController.Instance.musicManager.PlayClickSound();
        //contentVerticalLayoutGroup.CalculateLayoutInputVertical();
    }

    protected override IEnumerator ReloadScroll()
    {
        contentCanvasGruop.DOKill();
        contentCanvasGruop.alpha = 0;
        contentContentSizeFitter.enabled = false;
        // contentVerticalLayoutGroup.enabled = false;
        yield return new WaitForSeconds(0.05f);
        // contentVerticalLayoutGroup.enabled = true;

        contentContentSizeFitter.enabled = true;

        contentVerticalLayoutGroup.enabled = true;
        contentVerticalLayoutGroup.CalculateLayoutInputVertical();
        contentVerticalLayoutGroup.SetLayoutVertical();

        yield return new WaitForSeconds(0.15f);
        float detalY = contentRects.sizeDelta.y;
       // if (Mathf.Abs(Mathf.Abs(detalY) - Mathf.Abs(contentRects.localPosition.y)) > 1500)
        {
            contentRects.localPosition = new Vector3(contentRects.localPosition.x, -detalY, contentRects.localPosition.z);
        }
        actionReloadScroll = null;
        contentCanvasGruop.DOFade(1, 0.15f);
    }
}
