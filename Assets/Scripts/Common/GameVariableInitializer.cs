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

        Testing();
    }

    private async void Testing() {
        while(true) {
            testCXurrentLevel.text = currentLevel.Value.ToString();

            await Task.Delay(500);
        }
    }

    private void Start()
    {
        canvasScale.Value = canvas.localScale;
    }
}
