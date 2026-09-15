using UnityEngine;
using UnityEngine.UI;

public class VoyageMilestoneItemUI : MonoBehaviour
{
    public RectTransform container;
    public Image reward;
    public Button rewardButton;

    public void Init(VoyageMilestone voyageMilestone, RectTransform voyageUIContainer)
    {
        Vector2 position = Vector2.zero;

        container.SetParent(voyageUIContainer);
        container.localScale = Vector3.one;
        position.x = Mathf.Lerp(-0.5f * voyageUIContainer.sizeDelta.x, 0.5f * voyageUIContainer.sizeDelta.x, voyageMilestone.percent);

        container.localPosition = position;
    }
}
