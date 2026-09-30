using System.Collections.Generic;
using UnityEngine;

public class ResearchTreeUI : MonoBehaviour
{
    [SerializeField] private Transform itemContainer;
    [SerializeField] private List<ResearchNodeSO> researchNodes;

    private readonly List<ResearchItemUI> spawnedItems = new List<ResearchItemUI>();

    private void Awake()
    {
        ResearchManager.OnResearchStateChanged += RefreshList;
    }

    private void OnDestroy()
    {
        ResearchManager.OnResearchStateChanged -= RefreshList;
    }

    private void OnEnable()
    {
        RefreshList();
    }

    private void Update()
    {
        foreach (var item in spawnedItems)
        {
            item.UpdateUI();
        }
    }

    private void RefreshList()
    {
        CommonUtil.ReturnListToPool(spawnedItems, item => item.gameObject);

        foreach (var nodeSO in researchNodes)
        {
            ResearchItemUI itemUI = ObjectPoolingEverything.GetFromPool<ResearchItemUI>(GameConstants.researchItemUI);
            CommonUtil.SetParentAndResetScale(itemUI.transform, itemContainer);

            itemUI.Bind(nodeSO);
            spawnedItems.Add(itemUI);
        }
    }
}
