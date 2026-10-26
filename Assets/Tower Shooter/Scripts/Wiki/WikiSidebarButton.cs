using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WikiSidebarButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundHighlight;

    public void Setup(string pageTitle, int pageIndex, bool isSelected, System.Action<int> onClickAction)
    {
        if (titleText != null) titleText.text = pageTitle;
        
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickAction?.Invoke(pageIndex));
        }

        if (backgroundHighlight != null)
        {
            backgroundHighlight.gameObject.SetActive(isSelected);
        }
    }
}