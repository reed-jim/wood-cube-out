using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIRectLine : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0f, 0.5f); // Pivot at start of line
    }

    public void Connect(RectTransform startNode, RectTransform endNode)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        Vector3 startPos = startNode.position;
        Vector3 endPos = endNode.position;
        Vector3 direction = endPos - startPos;

        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rectTransform.position = startPos;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        rectTransform.sizeDelta = new Vector2(distance, rectTransform.sizeDelta.y);
    }
}
