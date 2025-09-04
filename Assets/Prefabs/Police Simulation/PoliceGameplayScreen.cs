using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoliceGameplayScreen : BasePopup
{
    [SerializeField] private Button pickRandomSituation;


    [Header("Day Settings")]
    [Tooltip("Length of full in-game day in real seconds")]
    public float dayDurationInSeconds = 60f;

    [Header("References")]
    public TMP_Text timeDisplay;

    private float timeOfDay;

    private bool _isPaused;
    private bool _isNoonBreakDone;

    protected override void RegisterMoreEvent()
    {
        unpauseDayTimeEvent += UnpauseDayTime;

        pickRandomSituation.onClick.AddListener(PickRandomSituation);
    }

    protected override void UnregisterMoreEvent()
    {
        unpauseDayTimeEvent -= UnpauseDayTime;
    }

    void Update()
    {
        if (_isPaused)
        {
            return;
        }

        timeOfDay += (Time.deltaTime / dayDurationInSeconds);

        if (timeOfDay > 1f)
        {
            SwitchRoute(GameEnum.ScreenRoute.DayEndPopup);

            _isPaused = true;
            _isNoonBreakDone = false;

            timeOfDay -= 1f;
        }

        if (timeDisplay != null)
        {
            int totalMinutes = Mathf.FloorToInt(timeOfDay * 1440);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            timeDisplay.text = string.Format("{0:00}:{1:00}", hours, minutes);
        }

        if (!_isNoonBreakDone)
        {
            if (timeOfDay >= 0.5f)
            {
                SwitchRoute(GameEnum.ScreenRoute.ShiftBreak);

                _isPaused = true;
                _isNoonBreakDone = true;
            }
        }
    }

    private void PickRandomSituation()
    {
        SwitchRoute(GameEnum.ScreenRoute.Decision);

        _isPaused = true;
    }

    private void UnpauseDayTime()
    {
        _isPaused = false;
    }
}
