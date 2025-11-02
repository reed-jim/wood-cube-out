using System;
using UnityEngine;

public class DotPuzzleInput : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    public static event Action unselectAllDotsEvent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectDot();
        }
    }

    void SelectDot()
    {
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Perform a 2D raycast at the mouse position
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            BaseDot dot = hit.collider.GetComponent<BaseDot>();

            if (dot != null)
            {
                dot.Select();
            }
        }
        else
        {
            unselectAllDotsEvent?.Invoke();
        }
    }
}
