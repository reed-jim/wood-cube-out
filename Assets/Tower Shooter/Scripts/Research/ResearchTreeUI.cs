using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class ResearchTreeUI : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform lineContainer;

    [Header("Spacing Settings")]
    [SerializeField] private Vector2 nodeSpacing = new Vector2(200f, 150f);

    [Header("Switchable Screen")]
    public SwitchableScreenType switchableScreenType;

    private readonly Dictionary<string, ResearchItemUI> spawnedNodes = new Dictionary<string, ResearchItemUI>();
    private readonly List<UIRectLine> spawnedLines = new List<UIRectLine>();

    private void Awake()
    {
        ResearchManager.OnResearchUpdated += RefreshTree;
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
    }

    private void OnDestroy()
    {
        ResearchManager.OnResearchUpdated -= RefreshTree;
        SwitchableScreenButton.switchScreenEvent -= OnScreenSwitch;
    }

    public void GenerateTree()
    {
        ClearTree();

        var configs = ResearchManager.Ins.AllResearchConfigs;

        // 1. Spawn and place Research Node UI items
        foreach (var config in configs)
        {
            ResearchItemUI itemUI = ObjectPoolingEverything.GetFromPool<ResearchItemUI>(GameConstants.researchItemUI);
            CommonUtil.SetParentAndResetScale(itemUI.transform, nodeContainer);

            RectTransform rect = itemUI.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(
                config.treePosition.x * nodeSpacing.x,
                config.treePosition.y * nodeSpacing.y
            );

            itemUI.Bind(config);
            spawnedNodes.Add(config.researchID, itemUI);
        }

        // 2. Draw prerequisite connection lines after layout positioning
        Canvas.ForceUpdateCanvases();
        DrawConnectionLines();
    }

    private void DrawConnectionLines()
    {
        foreach (var config in ResearchManager.Ins.AllResearchConfigs)
        {
            if (config.prerequisites == null || config.prerequisites.Count == 0) continue;

            if (!spawnedNodes.TryGetValue(config.researchID, out ResearchItemUI childNode)) continue;

            foreach (var prereq in config.prerequisites)
            {
                if (prereq == null) continue;

                if (spawnedNodes.TryGetValue(prereq.researchID, out ResearchItemUI parentNode))
                {
                    UIRectLine line = ObjectPoolingEverything.GetFromPool<UIRectLine>(GameConstants.UIRectLine);
                    CommonUtil.SetParentAndResetScale(line.transform, lineContainer);

                    line.Connect(
                        parentNode.GetComponent<RectTransform>(),
                        childNode.GetComponent<RectTransform>()
                    );

                    spawnedLines.Add(line);
                }
            }
        }
    }

    private void RefreshTree()
    {
        foreach (var node in spawnedNodes.Values)
        {
            node.UpdateUI();
        }
    }

    private void ClearTree()
    {
        CommonUtil.ReturnListToPool(new List<ResearchItemUI>(spawnedNodes.Values), item => item.gameObject);
        spawnedNodes.Clear();

        CommonUtil.ReturnListToPool(spawnedLines, line => line.gameObject);
        spawnedLines.Clear();
    }

    private void OnScreenSwitch(SwitchableScreenType switchableScreenType)
    {
        if (switchableScreenType == this.switchableScreenType)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        GenerateTree();
    }

    public void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }
}
