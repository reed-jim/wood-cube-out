using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JoinSpaceRacePopup : BasePopup
{
    [SerializeField] private Button joinButton;

    [SerializeField] private TMP_Text continueText;
    [SerializeField] private GameObject nextEventTimeContainer;
    [SerializeField] private TMP_Text timeCounterText;

    protected override void MoreActionInAwake()
    {
        joinButton.onClick.AddListener(Join);
    }

    private void Join()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        SwitchRoute(GameEnum.ScreenRoute.SpaceRace);
    }

    private void OnEnable()
    {
        bool isSpaceRaceAvailable = GameEventManager.Instance.IsSpaceRaceAvailable();

        joinButton.interactable = isSpaceRaceAvailable;
        joinButton.GetComponent<Image>().color = isSpaceRaceAvailable ? Color.white : Color.white.WithAlpha(0.5f);

        continueText.gameObject.SetActive(isSpaceRaceAvailable);
        nextEventTimeContainer.gameObject.SetActive(!isSpaceRaceAvailable);

        if (isSpaceRaceAvailable)
        {
            nextEventTimeContainer.gameObject.SetActive(false);
        }
        else
        {
            nextEventTimeContainer.gameObject.SetActive(true);

            StartCoroutine(CountNextEventTime());
        }
    }

    private IEnumerator CountNextEventTime()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            DateTime now = DateTime.UtcNow;

            int daysUntilFriday = ((int)DayOfWeek.Friday - (int)now.DayOfWeek + 7) % 7;

            DateTime endOfFriday = new DateTime(now.Year, now.Month, now.Day)
                .AddDays(daysUntilFriday)
                .AddHours(23)
                .AddMinutes(59)
                .AddSeconds(59);

            TimeSpan timeLeft = endOfFriday - now;

            double remainingSeconds = timeLeft.TotalSeconds;

            timeCounterText.text = $"{CommonUtil.ToHHMMSS(remainingSeconds)}";

            yield return waitForSeconds;
        }
    }
}
