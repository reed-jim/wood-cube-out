using UnityEngine;

public class PathSlot : MonoBehaviour
{
    public float percentInPath;

    public ClothingPiece clothingPiece;

    public bool isLeft;

    public void AddClothingPiece(ClothingPiece clothingPiece)
    {
        clothingPiece.transform.SetParent(transform);
        clothingPiece.transform.localPosition = Vector3.zero;
        clothingPiece.transform.localScale = Vector3.one;

        this.clothingPiece = clothingPiece;
    }

    public void Clear()
    {
        clothingPiece = null;
    }
}
