using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoChestController : MonoBehaviour
{
    private List<RewardElement> poolRewards;
    [SerializeField] private RewardElement elementPrefab;
    [SerializeField] private Transform parentPool;

    private List<RewardRandom> rewards;
    [SerializeField] private Transform contentInfo;
    public Image chest;
    private Canvas canvas;

    public void Init(List<RewardRandom> rewards, string sortingLayerName)
    {
        ClearPool();
        poolRewards = new List<RewardElement>(); 
        this.rewards = rewards;

        if (canvas != null)
        {
            canvas = transform.GetChild(0).GetComponent<Canvas>();
            canvas.sortingLayerName = sortingLayerName;
        }
    }

    public void ShowInfo()
    {
        contentInfo.gameObject.SetActive(true);
        ClearPool();
        for (int i = 0; i < rewards.Count; i++)
        {
            if (GameController.Instance.dataContain.giftDatabase.GetGift(rewards[i].typeItem, out Gift gtf))
            {
                var rw = GetElement();
                rw.Init(gtf.getGiftSprite, rewards[i].amount, isUnlimitedItem: rewards[i].isUnlimited, isAnim: false);
            }
        }
    }

    public void DisableInfo()
    {
        contentInfo.gameObject.SetActive(false);
    }    

    private RewardElement GetElement()
    {
        for (int i = 0; i < poolRewards.Count; i++)
        {
            if (!poolRewards[i].gameObject.activeSelf)
            {
                poolRewards[i].gameObject.SetActive(true);
                return poolRewards[i];
            }
        }

        var reward = Instantiate(elementPrefab, parentPool);
        poolRewards.Add(reward);
        return reward;
    }

    private void ClearPool()
    {
        if (poolRewards != null)
        {
            for (int i = 0; i < poolRewards.Count; i++)
            {
                poolRewards[i].gameObject.SetActive(false);
            }
        }
    }    
}
