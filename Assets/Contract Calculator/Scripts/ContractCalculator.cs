using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContractCalculator : MonoBehaviour
{
    [SerializeField] private TMP_InputField priceInput;
    [SerializeField] private TMP_InputField slInput;
    [SerializeField] private TMP_Text outputText;
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

        outputText.text = $"{floorOutput} <color=#FF0000>(-{floorSl}) \n<color=#005A00>{ceilOutput} <color=#FF0000>(-{ceilSl})";
    }
}
