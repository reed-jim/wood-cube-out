using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RescueStory
{
    public class RescueStoryInputManager : MonoBehaviour
    {
        public List<BaseTile> selectedTiles;

        private Camera _mainCamera;

        void Awake()
        {
            _mainCamera = Camera.main;

            selectedTiles = new List<BaseTile>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectTiles();
            }
        }

        private void SelectTiles()
        {
            if (IsClickedOnUI())
            {
                return;
            }

            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 999, layerMask: LayerMask.GetMask("Default"));

            if (hit.collider != null)
            {
                BaseTile tile = hit.collider.GetComponent<BaseTile>();

                if (tile != null)
                {
                    if (tile.isSelected)
                    {
                        tile.Deselect();

                        selectedTiles.Remove(tile);
                    }
                    else
                    {
                        tile.Select();

                        selectedTiles.Add(tile);
                    }

                    if (selectedTiles.Count == 2)
                    {
                        if (TileManager.Ins.HasValidPath(selectedTiles[0], selectedTiles[1]))
                        {
                            selectedTiles[0].Complete();
                            selectedTiles[1].Complete();
                        }
                        else
                        {
                            selectedTiles[0].Deselect();
                            selectedTiles[1].Deselect();
                        }

                        selectedTiles.Clear();
                    }
                }
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
}
