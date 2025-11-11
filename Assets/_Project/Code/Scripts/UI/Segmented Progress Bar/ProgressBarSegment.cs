using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    public class ProgressBarSegment : MonoBehaviour
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private Image backgroundImage;

        public Vector2 Size => background.sizeDelta;
        public Vector2 InitialSize => new Vector2(backgroundImage.sprite.rect.width, backgroundImage.sprite.rect.height);

        public void SetLocalPositionX(float positionX)
        {
            background.localPosition = new Vector2(positionX, background.localPosition.y);
        }

        public void SetAnchoredPositionX(float positionX)
        {
            background.anchoredPosition = new Vector2(positionX, background.localPosition.y);
        }

        public void SetSizeDeltaX(float sizeX)
        {
            background.sizeDelta = new Vector2(sizeX, background.sizeDelta.y);
        }
    }
}
