using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadLevelErrorScene : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button retryLoadLevelButton;
    [SerializeField] private TMP_Text infoText;

    private List<Tween> _tweens;

    public static event Action retryLoadLevelEvent;
    public static event Action retryLoadRemoteContentEvent;

    void Awake()
    {
        retryButton.onClick.AddListener(Retry);
        retryLoadLevelButton.onClick.AddListener(RetryLoadLevel);

        _tweens = new List<Tween>();

        canvasGroup.gameObject.SetActive(false);
        retryLoadLevelButton.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private void Retry()
    {
        retryLoadRemoteContentEvent?.Invoke();

        SetStateRetryButton();
    }

    private void RetryLoadLevel()
    {
        retryLoadLevelEvent?.Invoke();

        retryLoadLevelButton.interactable = false;
    }

    private void Show(bool isShow)
    {
        float endValue = 0;

        if (isShow)
        {
            endValue = 1;
        }

        if (isShow)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        _tweens.Add(Tween.Alpha(canvasGroup, endValue, duration: 0.3f)
        .OnComplete(() =>
        {
            if (!isShow)
            {
                canvasGroup.gameObject.SetActive(false);
            }
        })
        );

        SetStateRetryButton();

        StartCoroutine(CheckForInternetConnection());
    }

    private IEnumerator CheckForInternetConnection()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                retryButton.gameObject.SetActive(true);
            }

            yield return waitForSeconds;
        }
    }

    private void UpdateRemoteContentDownloadProgress(string text)
    {
        infoText.text = text;
    }

    private void SetStateRetryButton()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            retryButton.gameObject.SetActive(true);
        }
        else
        {
            retryButton.gameObject.SetActive(false);
        }
    }

    #region CALLBACK
    private void OnRemoteContentDownloaded()
    {
        retryLoadLevelButton.gameObject.SetActive(true);

        retryLoadLevelButton.interactable = false;

        retryLoadLevelButton.transform.localScale = Vector3.zero;

        _tweens.Add(Tween.Scale(retryLoadLevelButton.transform, 1.1f, duration: 0.3f)
        .OnComplete(() =>
        {
            _tweens.Add(Tween.Scale(retryLoadLevelButton.transform, 1f, duration: 0.15f)
            .OnComplete(() =>
            {
                retryLoadLevelButton.interactable = true;
            }));
        }));
    }
    #endregion 
}
