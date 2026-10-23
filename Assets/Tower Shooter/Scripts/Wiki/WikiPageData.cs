using UnityEngine;

[CreateAssetMenu(fileName = "NewWikiPage", menuName = "Wiki/Wiki Page")]
public class WikiPageData : ScriptableObject
{
    [SerializeField] private string articleTitle = "Article Title";
    [TextArea(6, 15)]
    [SerializeField] private string[] pageContents;

    public string ArticleTitle => articleTitle;
    public int TotalPages => pageContents != null ? pageContents.Length : 0;

    public string GetPageContent(int pageIndex)
    {
        if (pageContents == null || pageIndex < 0 || pageIndex >= pageContents.Length)
            return string.Empty;
        
        return pageContents[pageIndex];
    }
}