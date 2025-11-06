using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextFlicker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textToFlicker;
    [SerializeField] private float minAlpha = 0.4f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float flickerSpeed = 5f;

    private float currentAlpha = 1f;
    private bool isFlickering = true;

    void Update()
    {
        if (isFlickering)
        {
            currentAlpha += flickerSpeed * Time.deltaTime;

            if (currentAlpha > maxAlpha)
            {
                currentAlpha = maxAlpha;
                isFlickering = false;
            }

            textToFlicker.color = new Color(textToFlicker.color.r, textToFlicker.color.g, textToFlicker.color.b, currentAlpha);
        }
        else
        {
            currentAlpha -= flickerSpeed * Time.deltaTime;

            if (currentAlpha < minAlpha)
            {
                currentAlpha = minAlpha;
                isFlickering = true;
            }

            textToFlicker.color = new Color(textToFlicker.color.r, textToFlicker.color.g, textToFlicker.color.b, currentAlpha);
        }
    }
}