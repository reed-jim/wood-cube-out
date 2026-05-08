using UnityEngine;

public class LoopPathSlot : MonoBehaviour
{
    public float percentInLoopPath;

    public ClothingPiece clothingPiece;

    public void AddClothingPiece(ClothingPiece clothingPiece)
    {
        clothingPiece.transform.SetParent(transform);
        clothingPiece.transform.localPosition = Vector3.zero;

        this.clothingPiece = clothingPiece;
    }

    public void Clear()
    {
        clothingPiece = null;
    }
}
