using AntsFlow;
using UnityEngine;

namespace AntsFlow
{
    public class FakeTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Color faction;

        public Vector3 positionInAnt;
        public float scaleInAnt;

        public void Init(BaseAnt ant, Color faction)
        {
            spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(faction);

            transform.transform.SetParent(ant.transform);
            transform.localPosition = positionInAnt;
            transform.localScale = scaleInAnt * Vector3.one;
            transform.localRotation = Quaternion.identity;

            ant.fakeTile = this;
        }
    }
}
