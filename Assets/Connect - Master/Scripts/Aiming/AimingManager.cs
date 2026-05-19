using UnityEngine;
using UnityEngine.UI;

public class AimingManager : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Image aim;

    void Update()
    {
        Aiming();
    }

    private void Aiming()
    {
        if (Input.GetMouseButton(0))
        {
            aim.rectTransform.localPosition = Input.mousePosition - 0.5f * canvas.localScale.x * (Vector3)canvas.sizeDelta;
        }
    }
}
