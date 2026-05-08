using UnityEngine;

public class WaterFlowGameInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(
            worldPoint,
            Vector2.zero,
            0f
        );

            if (hit.collider != null)
            {
                WashingMachineItem washingMachineItem = hit.collider.GetComponent<WashingMachineItem>();

                if (washingMachineItem != null)
                {
                    washingMachineItem.Select();
                }
            }
        }
    }
}
