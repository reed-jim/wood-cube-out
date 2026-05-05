using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RescueStory
{
    public class RescueStoryInputManager : MonoBehaviour
    {
        public List<BaseTile> selectedTiles;
        public LineRenderer tilesMatchPath;

        private Camera _mainCamera;

        void Awake()
        {
            _mainCamera = Camera.main;

            selectedTiles = new List<BaseTile>();

            tilesMatchPath.gameObject.SetActive(false);
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
                        List<Vector2> path;

                        if (TileManager.Ins.HasValidPath(selectedTiles[0], selectedTiles[1], out path))
                        {
                            selectedTiles[0].Complete();
                            selectedTiles[1].Complete();

                            ShowMatchingPath(path);
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

        private void ShowMatchingPath(List<Vector2> path)
        {
            tilesMatchPath.gameObject.SetActive(true);
            tilesMatchPath.positionCount = path.Count;
            tilesMatchPath.SetPositions(CommonUtil.GetListVector3(path, -3f).ToArray());

            Color startColor = tilesMatchPath.startColor;
            Color endColor = tilesMatchPath.endColor;

            Tween.Custom(0, 1, duration: 0.3f, onValueChange: newVal =>
            {
                tilesMatchPath.startColor = startColor.ChangeAlpha(newVal);
                tilesMatchPath.endColor = endColor.ChangeAlpha(newVal);
            });

            Tween.Delay(0.4f).OnComplete(() =>
            {
                Tween.Custom(1, 0, duration: 0.2f, onValueChange: newVal =>
                {
                    tilesMatchPath.startColor = startColor.ChangeAlpha(newVal);
                    tilesMatchPath.endColor = endColor.ChangeAlpha(newVal);
                })
                .OnComplete(() =>
                {
                    tilesMatchPath.gameObject.SetActive(false);
                });
            });
        }
    }
}
