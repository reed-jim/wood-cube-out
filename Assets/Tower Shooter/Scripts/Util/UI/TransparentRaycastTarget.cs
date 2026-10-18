using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TransparentRaycastTarget : Graphic
{
    // Do not draw any color or texture
    protected override void UpdateGeometry() { }

    // This ensures the object is considered a raycast target for IPointerEnter/Exit,
    // but since it has no visual mesh/texture, it won't interfere visually.
    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        // Return true so pointer events (hover) still hit this object
        return true; 
    }
}