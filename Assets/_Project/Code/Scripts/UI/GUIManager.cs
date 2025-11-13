using TMPro;
using UnityEngine;

public class GUIManager : Singleton<GUIManager>
{
    [SerializeField] private TMP_FontAsset defaultFont;
    public TMP_FontAsset DefaultFont => defaultFont;
}
