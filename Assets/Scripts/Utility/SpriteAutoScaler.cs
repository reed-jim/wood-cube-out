using MoreMountains.Tools;
using NamPhuThuy;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAutoScaler : MonoBehaviour, MMEventListener<ELevelCameraUpdated>
{
    private SpriteRenderer _spriteRenderer;

    private void OnEnable()
    {
        MMEventManager.RegisterAllCurrentEvents(this);
    }

    private void OnDisable()
    {
        MMEventManager.UnregisterAllCurrentEvents(this);
    }

    void Start()
    {
        ScaleSpriteToScreen();
    }

    public void ScaleSpriteToScreen(Camera camera = null)
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

        if (GamePersistentVariable.screenSizeWorld == Vector2.zero)
        {
            GamePersistentVariable.screenSizeWorld = CommonUtil.GetScreenSizeWorld(camera);
        }

        float screenHeight = GamePersistentVariable.screenSizeWorld.y;
        float screenWidth = GamePersistentVariable.screenSizeWorld.x;

        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;

        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    public void OnMMEvent(ELevelCameraUpdated eventType)
    {
        ScaleSpriteToScreen();
    }
}
