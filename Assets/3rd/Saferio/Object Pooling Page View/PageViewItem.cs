using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageViewItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button selectItemButton;

    public RectTransform Container => container;

    public Vector2 Size => container.sizeDelta;

    void Awake()
    {
        selectItemButton.onClick.AddListener(Select);
    }

    public virtual void Setup(int index)
    {
        text.text = $"{index}";
    }

    protected virtual void Select()
    {

    }
}
