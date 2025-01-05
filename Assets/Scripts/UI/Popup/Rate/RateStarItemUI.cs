using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class RateStarItemUI : MonoBehaviour
{
    [SerializeField] private Image activeState;
    [SerializeField] private Button selectButton;

    [SerializeField] private int index;
    [SerializeField] private float transitionDuration;

    public static event Action<int> selectRateStarEvent;

    public int Index
    {
        get => index;
    }

    private void Awake()
    {
        selectRateStarEvent += OnRateStarSelected;

        selectButton.onClick.AddListener(Select);
    }

    private void OnDestroy()
    {
        selectRateStarEvent -= OnRateStarSelected;
    }

    public void ChangeState(bool _isActive)
    {
        if (_isActive)
        {
            activeState.gameObject.SetActive(true);

            Tween.Alpha(activeState, activeState.color.a, 1, duration: transitionDuration);
        }
        else
        {
            Tween.Alpha(activeState, activeState.color.a, 0, duration: transitionDuration)
            .OnComplete(() =>
            {
                activeState.gameObject.SetActive(false);
            });
        }
    }

    private void Select()
    {
        ChangeState(true);

        selectRateStarEvent?.Invoke(index);
    }

    private void OnRateStarSelected(int selectedIndex)
    {
        if (index < selectedIndex)
        {
            Select();
        }
    }
}
