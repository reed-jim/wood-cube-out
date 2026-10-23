using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWikiArticle", menuName = "Wiki/Wiki Article Data")]
public class WikiArticleData : ScriptableObject
{
    [SerializeField] private string articleTitle = "Article Title";
    [SerializeField] private List<WikiPage> pages = new List<WikiPage>();

    public string ArticleTitle => articleTitle;
    public int TotalPages => pages != null ? pages.Count : 0;

    public WikiPage GetPage(int index)
    {
        if (pages == null || index < 0 || index >= pages.Count) return null;
        return pages[index];
    }
}

[Serializable]
public class WikiPage
{
    [SerializeField] private string pageHeaderTitle; // Optional title for this specific page
    [SerializeField] private List<WikiBlock> contentBlocks = new List<WikiBlock>();

    public string PageHeaderTitle => pageHeaderTitle;
    public List<WikiBlock> ContentBlocks => contentBlocks;
}

[Serializable]
public struct WikiBlock
{
    public enum BlockType 
    { 
        Header, 
        Paragraph, 
        BulletPoint, 
        Image 
    }

    [Header("Block Configuration")]
    public BlockType type;

    [Tooltip("Used for Header, Paragraph, or Bullet Point text.")]
    [TextArea(2, 6)] 
    public string textContent;

    [Tooltip("Used when Block Type is set to Image.")]
    public Sprite imageSprite;
}