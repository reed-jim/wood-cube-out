using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AntsFlow
{
    public class FreezedTileRegion : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text antLeftText;

        public BoxCollider2D regionCollider;

        public List<BaseTile> tiles;
        public int antLeft;
        public bool isDone;

        void Awake()
        {
            BaseTile.tileCompleteEvent += OnTileCompleted;
        }

        void OnDestroy()
        {
            BaseTile.tileCompleteEvent -= OnTileCompleted;
        }

        private void OnTileCompleted(BaseTile tile)
        {
            antLeft--;

            antLeftText.text = $"{antLeft}";

            if (antLeft <= 0)
            {
                Complete();
            }
        }

        private void Complete()
        {
            SaferioTween.AlphaHide(canvasGroup);

            foreach (var tile in tiles)
            {
                tile.isLocked = false;
            }
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

            EditorUtility.SetDirty(region);
        }
    }
}
