using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAutoScaler : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        ScaleSpriteToScreen();
    }

    public void ScaleSpriteToScreen()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_spriteRenderer.sprite == null)
        {
            return;
        }

        // Maintain aspect ratio 
        Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;

        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    // public void OnMMEvent(ELevelCameraUpdated eventType)
    // {
    //     ScaleSpriteToScreen();
    // }
}
