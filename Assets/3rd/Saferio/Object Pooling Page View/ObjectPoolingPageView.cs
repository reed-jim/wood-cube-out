using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPoolingPageView : MonoBehaviour
{
    [SerializeField] private PageViewItem pageViewItemPrefab;
    [SerializeField] private List<PageViewItem> poolItems;

    [SerializeField] private RectTransform container;

    #region Paging
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private TMP_InputField pageInputField;
    #endregion

    [Header("Customize")]
    public int row;
    public int column;
    public float distanceRatio;

    private int currentPage;

    void Awake()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
        pageInputField.onSubmit.AddListener(text => GoToPage(int.Parse(text)));

        Init();
    }

    private void Init()
    {
        Vector2 itemSize = pageViewItemPrefab.Size;
        Vector2 distance = (1 + distanceRatio) * itemSize;

        float totalWidth = column * distance.x;
        float totalHeight = row * distance.y;

        Vector2 position;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int index = j + i * column;

                position.x = -((column - 1) / 2f) * distance.x + j * distance.x;
                position.y = ((row - 1) / 2f) * distance.y - i * distance.y;

                PageViewItem pageViewItem = Instantiate(pageViewItemPrefab, container);

                pageViewItem.Container.localPosition = position;

                pageViewItem.Setup(index);

                poolItems.Add(pageViewItem);
            }
        }
    }

    private void NextPage()
    {
        currentPage++;

        UpdatePage();
    }

    private void PrevPage()
    {
        if (currentPage == 0)
        {
            return;
        }

        currentPage--;

        UpdatePage();
    }

    private void UpdatePage()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int index = j + i * column;
                int itemIndex = index + currentPage * column * row;

                PageViewItem pageViewItem = poolItems[index];

                pageViewItem.Setup(itemIndex);
            }
        }
    }

    private void GoToPage(int page)
    {
        if (page < 0)
        {
            return;
        }

        currentPage = page;

        UpdatePage();
    }
}
