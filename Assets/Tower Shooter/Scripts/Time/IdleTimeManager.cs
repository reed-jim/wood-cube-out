using System;
using UnityEngine;

public class IdleTimeManager : Singleton<IdleTimeManager>
{
    [Header("Settings")]
    public float secondsPerInGameDay = 5f; // 5 real-time seconds = 1 in-game day
    public int currentYear = 2026;
    public int currentMonth = 1;
    public int currentDay = 1;

    [Header("Election Settings")]
    public int yearsBetweenElections = 4;
    public int nextElectionYear = 2030;

    public static event Action OnDayPassed;
    public static event Action OnYearPassed;
    public static event Action OnElectionStarted;

    private float _timer;

    public DateTime CurrentDateTime => new DateTime(currentYear, currentMonth, currentDay);

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= secondsPerInGameDay)
        {
            _timer -= secondsPerInGameDay;
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        currentDay++;
        if (currentDay > 30) // Simplified 30-day month model
        {
            currentDay = 1;
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
                OnYearPassed?.Invoke();
                CheckElection();
            }
        }

        OnDayPassed?.Invoke();
    }

    private void CheckElection()
    {
        if (currentYear >= nextElectionYear)
        {
            nextElectionYear = currentYear + yearsBetweenElections;
            OnElectionStarted?.Invoke();
        }
    }
}
