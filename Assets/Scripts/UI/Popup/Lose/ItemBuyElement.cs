using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBuyElement : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text textCount;

    public void SetData(bool isTime, int value)
    {
        gameObject.SetActive(true);

        if (isTime)
        {
            textCount.text = $"+{value}s";
        }
        else
        {
            textCount.text = $"x{value}";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
