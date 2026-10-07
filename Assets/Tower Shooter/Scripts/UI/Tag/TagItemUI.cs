using UnityEngine;
using UnityEngine.UI;

public class TagItemUI : MonoBehaviour
{
    [SerializeField] private Image border;
    [SerializeField] private ThemeableUI borderThemeable;

    public void SetBorderColor(Color color)
    {
        borderThemeable.enabled = false;
        border.color = color;
    }
}
