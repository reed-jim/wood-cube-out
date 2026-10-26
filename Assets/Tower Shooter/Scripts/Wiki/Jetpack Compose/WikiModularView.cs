// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class WikiModularView : MonoBehaviour
// {
//     [Header("Data Source")]
//     [SerializeField] private WikiArticleData currentArticle;

//     [Header("Root Layout Containers (Compose Tree)")]
//     [SerializeField] private WikiLayoutContainer mainContentLayoutContainer; // The container holding the main page column

//     [Header("UI Controls")]
//     [SerializeField] private TextMeshProUGUI articleTitleText;
//     [SerializeField] private TextMeshProUGUI pageCounterText;
//     [SerializeField] private Button prevButton;
//     [SerializeField] private Button nextButton;

//     [Header("Prefabs")]
//     [SerializeField] private GameObject sidebarButtonPrefab;
//     [SerializeField] private Transform sidebarColumnContainer;
//     [SerializeField] private WikiContentItem headerPrefab;
//     [SerializeField] private WikiContentItem paragraphPrefab;
//     [SerializeField] private WikiContentItem bulletPrefab;
//     [SerializeField] private WikiContentItem imagePrefab;

//     private int currentPageIndex = 0;
//     private readonly List<GameObject> activeInstances = new List<GameObject>();

//     private void Start()
//     {
//         prevButton?.onClick.AddListener(() => ChangePage(currentPageIndex - 1));
//         nextButton?.onClick.AddListener(() => ChangePage(currentPageIndex + 1));

//         if (currentArticle != null)
//         {
//             LoadArticle(currentArticle);
//         }
//     }

//     public void LoadArticle(WikiArticleData article)
//     {
//         currentArticle = article;
//         currentPageIndex = 0;
//         BuildSidebar();
//         RenderCurrentPage();
//     }

//     public void ChangePage(int newIndex)
//     {
//         if (currentArticle == null) return;
//         if (newIndex < 0 || newIndex >= currentArticle.TotalPages) return;

//         currentPageIndex = newIndex;
//         RenderCurrentPage();
//     }

//     private void BuildSidebar()
//     {
//         if (sidebarColumnContainer == null || sidebarButtonPrefab == null) return;

//         // Clear old sidebar items
//         foreach (Transform child in sidebarColumnContainer)
//         {
//             Destroy(child.gameObject);
//         }

//         // Populate Sidebar Column with navigation rows
//         for (int i = 0; i < currentArticle.TotalPages; i++)
//         {
//             int indexCapture = i;
//             WikiPage page = currentArticle.GetPage(i);

//             GameObject btnObj = Instantiate(sidebarButtonPrefab, sidebarColumnContainer);
//             var titleText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
//             if (titleText != null)
//             {
//                 titleText.text = !string.IsNullOrEmpty(page.PageHeaderTitle) ? page.PageHeaderTitle : $"Page {i + 1}";
//             }

//             var btn = btnObj.GetComponent<Button>();
//             if (btn != null)
//             {
//                 btn.onClick.AddListener(() => ChangePage(indexCapture));
//             }
//         }
//     }

//     private void RenderCurrentPage()
//     {
//         if (currentArticle == null) return;

//         // Update Header info
//         if (articleTitleText != null) articleTitleText.text = currentArticle.ArticleTitle;
//         if (pageCounterText != null) pageCounterText.text = $"{currentPageIndex + 1} / {currentArticle.TotalPages}";

//         if (prevButton != null) prevButton.interactable = (currentPageIndex > 0);
//         if (nextButton != null) nextButton.interactable = (currentPageIndex < currentArticle.TotalPages - 1);

//         // Get the current page and resolve its configuration (Page override takes priority, otherwise Global)
//         WikiPage page = currentArticle.GetPage(currentPageIndex);
//         if (page == null) return;

//         WikiLayoutConfig activeConfig = page.OverrideGlobalLayout 
//             ? page.PageLayoutConfig 
//             : currentArticle.GlobalLayoutConfig;

//         // Apply configuration to the layout container dynamically
//         ApplyLayoutConfiguration(mainContentLayoutContainer, activeConfig);

//         // Clear old content blocks in the main column
//         foreach (var instance in activeInstances)
//         {
//             if (instance != null) Destroy(instance);
//         }
//         activeInstances.Clear();

//         // Render new content blocks sequentially down the Column
//         if (mainContentLayoutContainer == null) return;

//         foreach (var block in page.ContentBlocks)
//         {
//             WikiContentItem prefab = GetPrefab(block.type);
//             if (prefab == null) continue;

//             WikiContentItem item = Instantiate(prefab, mainContentLayoutContainer.transform);
//             item.Setup(block);
//             activeInstances.Add(item.gameObject);
//         }
//     }

//     private void ApplyLayoutConfiguration(WikiLayoutContainer container, WikiLayoutConfig config)
//     {
//         if (container == null) return;

//         // Grab or target the underlying Unity Layout Group on the container
//         var layoutGroup = container.GetComponent<VerticalLayoutGroup>();
//         if (layoutGroup != null)
//         {
//             layoutGroup.spacing = config.spacing;
//             layoutGroup.padding = config.padding;
//             layoutGroup.childControlWidth = config.childControlWidth;
//             layoutGroup.childControlHeight = config.childControlHeight;
//         }
//     }

//     private WikiContentItem GetPrefab(WikiBlock.BlockType type) => type switch
//     {
//         WikiBlock.BlockType.Header => headerPrefab,
//         WikiBlock.BlockType.Paragraph => paragraphPrefab,
//         WikiBlock.BlockType.BulletPoint => bulletPrefab,
//         WikiBlock.BlockType.Image => imagePrefab,
//         _ => null
//     };
// }