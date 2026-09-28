using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class PooledCountryScrollView : MonoBehaviour
{
    [Header("Data & Prefab")]
    [SerializeField] private CountryFlagDatabase flagDatabase;
    [SerializeField] private CountryItemUI itemPrefab;
    [SerializeField] private RectTransform contentRect;

    [Header("Layout Settings")]
    [SerializeField] private float itemHeight = 60f;
    [SerializeField] private float itemSpacing = 5f;

    private ScrollRect scrollRect;
    private RectTransform viewportRect;

    private List<Country> availableCountries = new List<Country>();
    private List<CountryItemUI> activePool = new List<CountryItemUI>();

    private int totalCount;
    private int visibleItemCount;
    private int lastStartIndex = -1;

    private System.Action<Country> onCountrySelected;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewportRect = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    public void Initialize(System.Action<Country> onSelect)
    {
        onCountrySelected = onSelect;

        // Load all countries from database
        availableCountries.Clear();
        foreach (var entry in flagDatabase.flags)
        {
            availableCountries.Add(entry.country);
        }
        totalCount = availableCountries.Count;

        // Resize Content container to fit all items virtually
        float totalHeight = (totalCount * itemHeight) + (Mathf.Max(0, totalCount - 1) * itemSpacing);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        // Calculate pool size based on visible viewport space
        float viewportHeight = viewportRect.rect.height;
        visibleItemCount = Mathf.CeilToInt(viewportHeight / (itemHeight + itemSpacing)) + 2; // Extra buffer items

        CreatePool();
        RefreshView(forceUpdate: true);
    }

    private void CreatePool()
    {
        // Clear old pool if any exists
        foreach (var item in activePool)
        {
            Destroy(item.gameObject);
        }
        activePool.Clear();

        // Instantiate minimum required elements
        int spawnCount = Mathf.Min(visibleItemCount, totalCount);
        for (int i = 0; i < spawnCount; i++)
        {
            CountryItemUI instance = Instantiate(itemPrefab, contentRect);
            RectTransform rect = instance.GetComponent<RectTransform>();

            // Anchor setup for absolute positioning inside top-anchored Content
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, itemHeight);

            activePool.Add(instance);
        }
    }

    private void OnScroll(Vector2 position)
    {
        RefreshView(forceUpdate: false);
    }

    private void RefreshView(bool forceUpdate)
    {
        if (totalCount == 0) return;

        float contentY = contentRect.anchoredPosition.y;
        int startIndex = Mathf.FloorToInt(Mathf.Max(0, contentY) / (itemHeight + itemSpacing));
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalCount - activePool.Count));

        if (startIndex == lastStartIndex && !forceUpdate) return;
        lastStartIndex = startIndex;

        for (int i = 0; i < activePool.Count; i++)
        {
            int dataIndex = startIndex + i;
            if (dataIndex >= totalCount)
            {
                activePool[i].gameObject.SetActive(false);
                continue;
            }

            activePool[i].gameObject.SetActive(true);

            // Reposition item
            float posY = -(dataIndex * (itemHeight + itemSpacing));
            RectTransform rect = activePool[i].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, posY);

            // Bind data
            Country country = availableCountries[dataIndex];
            Sprite flag = flagDatabase.GetFlag(country);
            activePool[i].Setup(country, flag, onCountrySelected);
        }
    }
}
