using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DailyRewardCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text timeCounterText;

    void Awake()
    {
        StartCoroutine(Counting());
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateRemainingTime();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        DateTime now = DateTime.UtcNow;

        DateTime endOfDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc);

        TimeSpan timeLeft = endOfDay - now;

        double remainingSeconds = timeLeft.TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ToHHMMSS(remainingSeconds)}";
    }
}
