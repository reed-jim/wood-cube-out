using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameVariableInitializer : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;

    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasScale;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action currentLevelFetchedEvent;

    [SerializeField] private TMP_Text testCXurrentLevel;

    private void Awake()
    {
        canvasSize.Value = canvas.sizeDelta;

        currentLevel.Load();

        userResourcesObserver.Load();

        currentLevelFetchedEvent?.Invoke();

        // Testing();
    }

    private async void Testing()
    {
        await Task.Delay(1000);

        int time = 0;

        while (true)
        {
            string text = userResourcesObserver.UserResources.BoosterQuantities.ToString();

            text += " / " + userResourcesObserver.UserResources.BoosterQuantities[1].ToString();
            text += " / " + userResourcesObserver.UserResources.BoosterQuantities[2].ToString();
            text += " / " + time;

            testCXurrentLevel.text = text;

            await Task.Delay(500);

            time++;
        }
    }

    private void Start()
    {
        canvasScale.Value = canvas.localScale;
    }
}
