using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WikiLayoutContainer : MonoBehaviour
{
    public enum LayoutType { Row, Column }
    
    [Header("Layout Configuration")]
    [SerializeField] private LayoutType layoutType;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private RectOffset padding = new RectOffset(10, 10, 10, 10);

    private HorizontalOrVerticalLayoutGroup layoutGroup;

    private void Awake()
    {
        SetupLayoutGroup();
    }

    private void SetupLayoutGroup()
    {
        // Automatically add or configure the correct Unity layout group based on type
        if (layoutType == LayoutType.Row)
        {
            var hGroup = GetComponent<HorizontalLayoutGroup>() ?? gameObject.AddComponent<HorizontalLayoutGroup>();
            hGroup.spacing = spacing;
            hGroup.padding = padding;
            hGroup.childControlWidth = true;
            hGroup.childControlHeight = true;
            hGroup.childScaleWidth = false;
            hGroup.childScaleHeight = false;
            layoutGroup = hGroup;
        }
        else
        {
            var vGroup = GetComponent<VerticalLayoutGroup>() ?? gameObject.AddComponent<VerticalLayoutGroup>();
            vGroup.spacing = spacing;
            vGroup.padding = padding;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;
            vGroup.childScaleWidth = false;
            vGroup.childScaleHeight = false;
            layoutGroup = vGroup;
        }

        // Ensure a content size fitter exists for scalability
        var sizeFitter = GetComponent<ContentSizeFitter>() ?? gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // Modular method to append child elements programmatically
    public void AddChild(GameObject child)
    {
        if (child != null)
        {
            child.transform.SetParent(transform, false);
        }
    }
}