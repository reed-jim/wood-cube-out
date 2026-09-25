using System;
using UnityEngine;
using UnityEngine.UI;

public class SwitchableScreenButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button switchButton;

    [Header("Customize")]
    public SwitchableScreenType switchableScreenType;

    public static event Action<SwitchableScreenType> switchScreenEvent;

    void Awake()
    {
        switchButton.onClick.AddListener(SwitchScreen);
    }

    private void SwitchScreen()
    {
        switchScreenEvent?.Invoke(switchableScreenType);
    }
}
