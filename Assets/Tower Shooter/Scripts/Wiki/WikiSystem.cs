using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WikiSystem : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private WikiArticleData currentArticle;

    [Header("UI References (Main Pane)")]
    [SerializeField] private TextMeshProUGUI mainTitleText;
    [SerializeField] private TextMeshProUGUI pageCounterText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Transform contentParent;

    [Header("GitBook Sidebar References")]
    [SerializeField] private Transform sidebarContentParent;
    [SerializeField] private GameObject sidebarButtonPrefab; // Prefab with WikiSidebarButton attached

    [Header("Content Item Prefabs")]
    [SerializeField] private WikiContentItem headerPrefab;
    [SerializeField] private WikiContentItem paragraphPrefab;
    [SerializeField] private WikiContentItem bulletPrefab;
    [SerializeField] private WikiContentItem imagePrefab;

    public event Action<int, int> OnPageChanged;

    private int currentPageIndex = 0;
    private readonly List<GameObject> spawnedBlockObjects = new List<GameObject>();
    private readonly List<GameObject> spawnedSidebarButtons = new List<GameObject>();

    private void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);

        LoadArticle(currentArticle);
    }

    public void LoadArticle(WikiArticleData article)
    {
        currentArticle = article;
        currentPageIndex = 0;
        BuildSidebar();
        RefreshUI();
    }

    public void GoToPage(int index)
    {
        if (currentArticle != null && index >= 0 && index < currentArticle.TotalPages)
        {
            currentPageIndex = index;
            RefreshUI();
        }
    }

    public void NextPage()
    {
        if (currentArticle != null && currentPageIndex < currentArticle.TotalPages - 1)
        {
            currentPageIndex++;
            RefreshUI();
        }
    }

    public void PrevPage()
    {
        if (currentArticle != null && currentPageIndex > 0)
        {
            currentPageIndex--;
            RefreshUI();
        }
    }

    private void BuildSidebar()
    {
        // Clear old sidebar buttons
        foreach (var btn in spawnedSidebarButtons)
        {
            if (btn != null) Destroy(btn);
        }
        spawnedSidebarButtons.Clear();

        if (currentArticle == null || sidebarContentParent == null || sidebarButtonPrefab == null) return;

        for (int i = 0; i < currentArticle.TotalPages; i++)
        {
            WikiPage page = currentArticle.GetPage(i);
            GameObject btnObj = Instantiate(sidebarButtonPrefab, sidebarContentParent);
            
            WikiSidebarButton sidebarBtn = btnObj.GetComponent<WikiSidebarButton>();
            if (sidebarBtn != null)
            {
                string displayTitle = !string.IsNullOrEmpty(page.PageHeaderTitle) ? page.PageHeaderTitle : $"Page {i + 1}";
                bool isSelected = (i == currentPageIndex);
                sidebarBtn.Setup(displayTitle, i, isSelected, GoToPage);
            }

            spawnedSidebarButtons.Add(btnObj);
        }
    }

    private void UpdateSidebarSelection()
    {
        for (int i = 0; i < spawnedSidebarButtons.Count; i++)
        {
            var sidebarBtn = spawnedSidebarButtons[i].GetComponent<WikiSidebarButton>();
            if (sidebarBtn != null)
            {
                WikiPage page = currentArticle.GetPage(i);
                string displayTitle = !string.IsNullOrEmpty(page.PageHeaderTitle) ? page.PageHeaderTitle : $"Page {i + 1}";
                bool isSelected = (i == currentPageIndex);
                sidebarBtn.Setup(displayTitle, i, isSelected, GoToPage);
            }
        }
    }

    private void RefreshUI()
    {
        if (currentArticle == null || currentArticle.TotalPages == 0)
        {
            ClearUI();
            return;
        }

        if (mainTitleText != null) mainTitleText.text = currentArticle.ArticleTitle;
        if (pageCounterText != null) 
        {
            pageCounterText.text = $"Page {currentPageIndex + 1} / {currentArticle.TotalPages}";
        }

        if (nextButton != null) nextButton.interactable = (currentPageIndex < currentArticle.TotalPages - 1);
        if (prevButton != null) prevButton.interactable = (currentPageIndex > 0);

        UpdateSidebarSelection();
        BuildPageContent(currentArticle.GetPage(currentPageIndex));

        OnPageChanged?.Invoke(currentPageIndex + 1, currentArticle.TotalPages);
    }

    private void BuildPageContent(WikiPage page)
    {
        // Clear old block instances
        foreach (var obj in spawnedBlockObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedBlockObjects.Clear();

        if (page == null || contentParent == null) return;

        foreach (var block in page.ContentBlocks)
        {
            WikiContentItem prefabToSpawn = GetPrefabForBlockType(block.type);
            if (prefabToSpawn == null) continue;

            WikiContentItem itemInstance = Instantiate(prefabToSpawn, contentParent);
            itemInstance.Setup(block);

            spawnedBlockObjects.Add(itemInstance.gameObject);
        }
    }

    private WikiContentItem GetPrefabForBlockType(WikiBlock.BlockType type)
    {
        return type switch
        {
            WikiBlock.BlockType.Header => headerPrefab,
            WikiBlock.BlockType.Paragraph => paragraphPrefab,
            WikiBlock.BlockType.BulletPoint => bulletPrefab,
            WikiBlock.BlockType.Image => imagePrefab,
            _ => null
        };
    }

    private void ClearUI()
    {
        foreach (var obj in spawnedBlockObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedBlockObjects.Clear();

        if (mainTitleText != null) mainTitleText.text = string.Empty;
        if (pageCounterText != null) pageCounterText.text = string.Empty;
        if (nextButton != null) nextButton.interactable = false;
        if (prevButton != null) prevButton.interactable = false;
    }
}