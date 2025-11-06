using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewHelper : MonoBehaviour
{
    [Header("Data")] 
    [SerializeField] private SamplePlayerData _samplePlayerData;

    [Header("View")] 
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _contentElementPrefab;
    [SerializeField] private GridLayoutGroup _contentGridLayoutGroup;
    [SerializeField] private ScrollRect _scrollRect;

    private Vector2 _elementSpacing;
    private Vector2 _elementSize;
    private Vector2 _contentBuffer = new Vector2(0f, 350f); 
    
    void Start()
    {
        Setup();
        
        //Update content in scroll view
        UpdateScrollViewContent();
    }

    private void Setup()
    {
        //GET REFERENCES
        _contentGridLayoutGroup = _content.GetComponent<GridLayoutGroup>();
        _scrollRect = GetComponent<ScrollRect>();

        //GET THE SIZES
        _elementSize = _contentGridLayoutGroup.cellSize;
        _elementSpacing = _contentGridLayoutGroup.spacing;

        _elementSize = new Vector2(_contentGridLayoutGroup.cellSize.x * (_scrollRect.horizontal ? 1 : 0), _contentGridLayoutGroup.cellSize.y * (_scrollRect.vertical ? 1 : 0));
        
        // Debug.Log($"element size: {_elementSize}");
        
        //Calculate the size of content-view
        _content.sizeDelta = _samplePlayerData.PlayerDatas.Count * _elementSize +
                             (_samplePlayerData.PlayerDatas.Count + 1) * _elementSpacing + 
                             _contentBuffer;
    }

    public void UpdateScrollViewContent()
    {
        //Resize the content-view-size
        
        
        
        //Update content
        foreach (PlayerData1 p in _samplePlayerData.PlayerDatas)
        {
            GameObject element = Instantiate(_contentElementPrefab, _content.transform);
            ContentElement contentElement = element.GetComponent<ContentElement>();
            
            //Assign value 
            contentElement.Name = p.name;
            contentElement.Avatar = p.avatar;
            contentElement.Level = p.level;
            contentElement.Rank = p.rank;

            // element.transform.parent = _content.transform;
            element.SetActive(true);
        }
        
    }
}
