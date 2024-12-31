using System;
using UnityEngine;

public class GameEnum : MonoBehaviour
{
    public enum ScreenRoute
    {
        Waiting,
        Lobby,
        LobbyRoom,
        Screen1,
        Screen2,
        Screen3,
        Screen4,
        Screen5,
        Setting,
        Win,
        Debug,
        LuckyWheel,
        WeeklyTask,
        IAPShop,
        Pause,
        RemoveAd,
        Lose
    }

    public enum GameFaction
    {
        Red,
        Blue,
        Green,
        Purple,
        Orange,
        Yellow,
        LightBlue,
        Pink,
        All,
        None
    }

    public enum CharacterAnimationState
    {
        Idle = 0,
        Walking = 1
    }

    public enum LevelDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    public enum InputMode
    {
        Select,
        BreakObject,
        Disabled
    }

    public enum TaskType
    {
        Uncrew,
        CompleteLevel,
    }

    public static T GetRandomEnumValue<T>()
    {
        Array enumValues = Enum.GetValues(typeof(T));

        System.Random random = new System.Random();

        return (T)enumValues.GetValue(random.Next(enumValues.Length));
    }
}
