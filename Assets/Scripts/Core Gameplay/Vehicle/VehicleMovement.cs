using PrimeTween;
using UnityEngine;
using UnityEngine.AI;

public class VehicleMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent vehicleNavMesh;

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward * 10);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (CheckFrontObstacle())
            {
                Tween.Scale(transform, 1.05f, duration: 0.2f, cycles: 6, cycleMode: CycleMode.Yoyo);

                return;
            }

            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray, out hit);

            if (hit.point != null)
            {
                vehicleNavMesh.destination = hit.point;
            }
        }
    }

    private bool CheckFrontObstacle()
    {
        RaycastHit obstacle;

        Physics.Raycast(transform.position, transform.forward, out obstacle, 10);

        if (obstacle.collider == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
