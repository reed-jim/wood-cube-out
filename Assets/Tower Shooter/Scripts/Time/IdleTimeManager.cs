using System;
using UnityEngine;

public class IdleTimeManager : Singleton<IdleTimeManager>
{
    [Header("Settings")]
    public float secondsPerInGameDay = 5f;
    public int currentYear = 2026;
    public int currentMonth = 1;
    public int currentDay = 1;

    [Header("Election Settings")]
    public int yearsBetweenElections = 4;
    public int nextElectionYear = 2026;

    [Tooltip("Specific month for election day (1-12)")]
    public int electionMonth = 3;

    [Tooltip("Specific day of the month for election day (1-30)")]
    public int electionDay = 1;

    public static event Action OnDayPassed;
    public static event Action OnYearPassed;
    public static event Action OnElectionStarted;

    private float _timer;

    private GameTimeData gameTimeData;

    public DateTime CurrentDateTime => new DateTime(currentYear, currentMonth, currentDay);

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LoadGameTimeData();
    }

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
        if (currentDay > 30)
        {
            currentDay = 1;
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
                OnYearPassed?.Invoke();
            }
        }

        // Check if today matches the scheduled election date
        CheckElectionDate();

        OnDayPassed?.Invoke();

        SaveGameTimeData();
    }

    private void CheckElectionDate()
    {
        if (currentYear == nextElectionYear && currentMonth == electionMonth && currentDay == electionDay)
        {
            OnElectionStarted?.Invoke();
        }
    }

    public void ScheduleNextElection()
    {
        nextElectionYear = currentYear + yearsBetweenElections;
    }

    #region Load/Save
    public void LoadGameTimeData()
    {
        gameTimeData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("gameTimeData"), GetDefaultGameTimeData());

        currentDay = gameTimeData.currentDay;
        currentMonth = gameTimeData.currentMonth;
        currentYear = gameTimeData.currentYear;
        nextElectionYear = gameTimeData.nextElectionYear;
    }

    public void SaveGameTimeData()
    {
        gameTimeData.currentDay = currentDay;
        gameTimeData.currentMonth = currentMonth;
        gameTimeData.currentYear = currentYear;
        gameTimeData.nextElectionYear = nextElectionYear;

        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("gameTimeData"), gameTimeData);
    }

    public GameTimeData GetDefaultGameTimeData()
    {
        GameTimeData gameTimeData = new GameTimeData();

        gameTimeData.currentDay = 1;
        gameTimeData.currentMonth = 1;
        gameTimeData.currentYear = 2026;

        return gameTimeData;
    }
    #endregion

    #region Util
    public string GetCurrentDateTime()
    {
        return CurrentDateTime.ToString("yyyy-MM-dd");
    }
    #endregion
}

public class GameTimeData
{
    public int currentYear;
    public int currentMonth;
    public int currentDay;
    public int nextElectionYear;
}
