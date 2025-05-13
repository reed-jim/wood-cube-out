using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour
{
    #region PRIVATE FIELD
    private Camera _mainCamera;
    private BaseItem _selectedItem;
    #endregion

    void Update()
    {
        if (_selectedItem == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectItem();
            }
        }
        else
        {
            MoveItem();
        }
    }

    private void SelectItem()
    {
        if (IsClickedOnUI())
        {
            return;
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            BaseItem item = hit.collider.GetComponent<BaseItem>();

            if (item != null)
            {
                if (item.LAYER == 0)
                {
                    _selectedItem = item;
                }
            }
        }
    }

    private void MoveItem()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            _selectedItem.Follow(mousePosition);
        }
        else
        {
            _selectedItem.Stop();

            _selectedItem = null;
        }
    }

    private bool IsClickedOnUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
