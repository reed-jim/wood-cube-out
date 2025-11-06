using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class AnimationUtils
{
    public static Tweener OpenDropdown(RectTransform dropdown, CanvasGroup dropdownCanvasGroup)
    {
        dropdown.gameObject.SetActive(true);

        float topPosition = dropdown.localPosition.y + 0.5f * dropdown.sizeDelta.y;

        dropdown.localScale = new Vector3(1, 0, 1);

        Vector3 initialPosition = dropdown.localPosition;
        Vector3 localPosition = dropdown.localPosition;

        dropdownCanvasGroup.interactable = false;

        Tweener tweener = dropdown.DOScaleY(1, duration: 0.3f).OnUpdate(() =>
        {
            localPosition.y = topPosition - 0.5f * dropdown.sizeDelta.y * dropdown.localScale.y;

            dropdown.localPosition = localPosition;
        })
        .OnComplete(() =>
        {
            dropdown.localPosition = initialPosition;

            dropdownCanvasGroup.interactable = true;
        });

        return tweener;
    }

    public static List<Tweener> HideDropdown(RectTransform dropdown, CanvasGroup dropdownCanvasGroup)
    {
        List<Tweener> tweeners = new List<Tweener>();

        float topPosition = dropdown.localPosition.y + 0.5f * dropdown.sizeDelta.y;

        dropdownCanvasGroup.interactable = false;

        tweeners.Add(dropdownCanvasGroup.DOFade(0, duration: 0.15f));

        Vector3 initialPosition = dropdown.localPosition;
        Vector3 localPosition = dropdown.localPosition;

        tweeners.Add(dropdown.DOScaleY(0, duration: 0.3f)
        .OnUpdate(() =>
        {
            localPosition.y = topPosition - 0.5f * dropdown.sizeDelta.y * dropdown.localScale.y;

            dropdown.localPosition = localPosition;
        })
        .OnComplete(() =>
        {
            dropdown.localPosition = initialPosition;
            dropdown.localScale = Vector3.one;
            dropdownCanvasGroup.alpha = 1;

            dropdownCanvasGroup.interactable = true;

            dropdown.gameObject.SetActive(false);
        }));

        return tweeners;
    }
}