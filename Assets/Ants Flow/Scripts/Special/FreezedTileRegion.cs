using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AntsFlow
{
    public class FreezedTileRegion : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer container;
        [SerializeField] private TMP_Text antLeftText;

        public BoxCollider2D regionCollider;

        public List<BaseTile> tiles;
        public int antLeft;
        public bool isDone;
        private bool isAnimating;

        void Awake()
        {
            BaseTile.tileCompleteEvent += OnTileCompleted;
        }

        void OnDestroy()
        {
            BaseTile.tileCompleteEvent -= OnTileCompleted;
        }

        void Start()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].isLocked = true;
            }

            antLeft = tiles.Count;
            antLeftText.text = $"{antLeft}";
        }

        private void OnTileCompleted(BaseTile tile)
        {
            antLeft--;
            antLeftText.text = $"{antLeft}";

            if (!isAnimating)
            {
                SaferioTween.Punch(antLeftText.transform, 1.1f, onCompleteAction: () =>
                {
                    isAnimating = false;
                });

                isAnimating = true;
            }

            if (antLeft <= 0)
            {
                Complete();
            }
        }

        private void Complete()
        {
            SaferioTween.AlphaHide(antLeftText);
            SaferioTween.AlphaHide(container);

            foreach (var tile in tiles)
            {
                tile.isLocked = false;
            }
        }

        public void MoveToCenterOfTiles()
        {
            Vector3 center = new Vector3();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                center += tile.transform.position;
            }

            center /= tiles.Count;

            transform.position = center.AddZ(-1);
        }
    }

    [CustomEditor(typeof(FreezedTileRegion))]
    public class FreezedTileRegionEditor : Editor
    {
        private FreezedTileRegion region;

        private void OnEnable()
        {
            region = (FreezedTileRegion)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("Find Tiles"))
            {
                FindTiles(region);
            }
        }

        private void FindTiles(FreezedTileRegion region)
        {
            Vector2 point = region.transform.position;
            Vector2 size = region.regionCollider.size;

            List<Collider2D> results = new List<Collider2D>();

            region.regionCollider.Overlap(results);

            region.tiles = new List<BaseTile>();

            for (int i = 0; i < results.Count; i++)
            {
                BaseTile tile = results[i].GetComponent<BaseTile>();

                if (tile != null)
                {
                    region.tiles.Add(tile);
                }
            }

            region.MoveToCenterOfTiles();

            EditorUtility.SetDirty(region);
        }
    }
}
