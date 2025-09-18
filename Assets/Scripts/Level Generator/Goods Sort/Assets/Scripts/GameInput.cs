using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour
{
    #region PRIVATE FIELD
    private Camera _mainCamera;
    private BaseItem _selectedItem;
    private BaseItem _itemTrail;
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
                if (item.LAYER == 0 && !item.IsLocked())
                {
                    _selectedItem = item;

                    _selectedItem.ItemServiceLocator.itemMaterialPropertyBlock.ShowOutline(true);

                    LeaveItemTrail(_selectedItem);
                }
            }
        }
    }

    private void LeaveItemTrail(BaseItem item)
    {
        _itemTrail = ObjectPoolingEverything.GetFromPool<BaseItem>(GameConstants.itemTrail);

        _itemTrail.gameObject.SetActive(true);
        _itemTrail.transform.SetParent(item.transform.parent);

        _itemTrail.transform.position = item.transform.position;
        _itemTrail.transform.localScale = item.transform.localScale;
        _itemTrail.ItemServiceLocator.itemVisual.SetFaction(item.TYPE);
        _itemTrail.ItemServiceLocator.itemVisual.SetAlpha(0.5f);
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
            _selectedItem.ItemServiceLocator.itemMaterialPropertyBlock.ShowOutline(false);

            _selectedItem = null;

            _itemTrail.ItemServiceLocator.itemVisual.FadeOut(() =>
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.itemTrail, _itemTrail.gameObject);
            });
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
