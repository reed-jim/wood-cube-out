using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectVehicle();
        }
    }

    private void SelectVehicle()
    {
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out hit);

        if (hit.collider != null)
        {
            BaseVehicle vehicle = hit.collider.GetComponentInParent<BaseVehicle>();

            if (vehicle != null)
            {
                vehicle.Park();
            }
        }
    }
}
