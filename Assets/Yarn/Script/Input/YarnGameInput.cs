using UnityEngine;

public class YarnGameInput : MonoBehaviour
{
    private Camera _mainCamera;

    void Awake()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectPiece();
        }
    }

    private void SelectPiece()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, maxDistance: 999, layerMask: LayerMask.GetMask("Default"));

        if (hit.collider != null)
        {
            BasePiece piece = hit.collider.GetComponent<BasePiece>();

            if (piece != null)
            {
                piece.PullYarn();

                return;
            }
        }
    }
}
