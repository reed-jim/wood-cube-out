using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class SettingItemUI : MonoBehaviour
{
    [SerializeField] private Image activeState;
    [SerializeField] private Image inactiveState;
    [SerializeField] private Image cross;
    [SerializeField] private Button selectButton;

    public bool isActive;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    private void Select()
    {
        isActive = !isActive;

        if (isActive)
        {
            SaferioTween.AlphaShow(activeState);
            SaferioTween.AlphaHide(inactiveState);
            SaferioTween.AlphaHide(cross);
        }
        else
        {
            SaferioTween.AlphaHide(activeState);
            SaferioTween.AlphaShow(inactiveState);
            SaferioTween.AlphaShow(cross);
        }
    }
}
