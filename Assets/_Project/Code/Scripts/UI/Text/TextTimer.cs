using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextTimer : MonoBehaviour
{
    private enum TimerType
    {
        CountDown,
        CountUp
    }

    [SerializeField] private TimerType _timerType; 
    [SerializeField] private TextMeshProUGUI _displayTime;
    
    [Header("Time")]
    [SerializeField] private int _seconds; 
    [SerializeField] private int _minutes; 
    [SerializeField] private int _hours; 
    [SerializeField] private int _days; 
    [SerializeField] private float inGameTimeScale = 80f;

    private int _coefficient;
    private float _inGameTime = 0f;
    private float _timeStep = 1f;

    private void Awake()
    {
        _displayTime = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        switch (_timerType)
        {
            case TimerType.CountDown:
                _coefficient = 1;
                break;
            case TimerType.CountUp:
                _coefficient = -1; 
                break;
        }
        StartCoroutine(UpdateTime());
        
    }

    private IEnumerator UpdateTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(_timeStep);
            _inGameTime += _timeStep * inGameTimeScale * _coefficient; 

            int hours = Mathf.FloorToInt(_inGameTime / 3600);
            int minutes = Mathf.FloorToInt((_inGameTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(_inGameTime % 60);

            _displayTime.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}
