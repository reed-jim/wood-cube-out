using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContractCalculator : MonoBehaviour
{
    [SerializeField] private TMP_InputField priceInput;
    [SerializeField] private TMP_InputField slInput;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_Text recommendText;
    [SerializeField] private Button calculateButton;

    void Awake()
    {
        calculateButton.onClick.AddListener(Calculate);
    }

    private void Calculate()
    {
        // 1 price in 1 mgc = 10
        float priceDifference = float.Parse(priceInput.text);
        float sl = float.Parse(slInput.text);

        int floorOutput = Mathf.FloorToInt(sl / (priceDifference * 10));
        int ceilOutput = Mathf.CeilToInt(sl / (priceDifference * 10));

        int floorSl = (int)((priceDifference * 10) * floorOutput);
        int ceilSl = (int)((priceDifference * 10) * ceilOutput);

        outputText.text = $"{floorOutput} <color=#FF0000>(-{floorSl})\n</color>{ceilOutput} <color=#FF0000>(-{ceilSl})</color>";

        if (floorOutput < 10)
        {
            recommendText.gameObject.SetActive(false);
        }
        else
        {
            recommendText.gameObject.SetActive(true);
            recommendText.text = "Enter macro instead";
        }
    }
}
