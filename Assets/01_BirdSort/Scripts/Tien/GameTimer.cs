using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : SingletonMonoBehaviour<GameTimer>
{
    private float _startTime;
    public float ElapsedTime => Time.time - _startTime;
    public int SecondsTime => (int)ElapsedTime;
    
    public void StartTimer()
    {
        _startTime = Time.time;
    }
}