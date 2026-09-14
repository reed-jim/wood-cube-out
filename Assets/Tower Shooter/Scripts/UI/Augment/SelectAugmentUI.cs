using PrimeTween;
using TMPro;
using UnityEngine;

public class SelectAugmentUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_Text descriptionText;

    public void Show(bool isSelectNode)
    {
        canvasGroup.gameObject.SetActive(true);

        if (isSelectNode)
        {
            descriptionText.text = "Select a node to apply augment...";
        }
        else
        {
            descriptionText.text = "Select a node group to apply augment...";
        }

        Tween.Alpha(canvasGroup, 1, duration: 0.3f);
    }

    public void Hide()
    {
        Tween.Alpha(canvasGroup, 0, duration: 0.3f).OnComplete(() =>
        {
            canvasGroup.gameObject.SetActive(false);
        });
    }
}
