using UnityEngine;
using UnityEngine.UI;

public class SaferioSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fill;

    void Awake()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnEnable()
    {
        fill.fillAmount = slider.value;
    }

    private void OnSliderValueChanged(float value)
    {
        fill.fillAmount = value;
    }
}
