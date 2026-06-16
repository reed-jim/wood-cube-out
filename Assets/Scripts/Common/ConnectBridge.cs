/// <summary>
/// Static bridge truyền dữ liệu giữa BirdSort và Connect scene.
/// Không phụ thuộc MonoBehaviour, tồn tại xuyên suốt vòng đời app.
/// </summary>
public static class ConnectBridge
{
    // ── Navigation flag ──────────────────────────────────────────────
    /// <summary>true khi Connect scene được launch từ BirdSort Home.</summary>
    public static bool IsLaunchedFromBirdSort { get; set; } = false;

    // ── Level ─────────────────────────────────────────────────────────
    /// <summary>Level index BirdSort hiện tại (1-based) sẽ chơi trong Connect.</summary>
    public static int BirdSortLevelOnEnterConnect { get; set; } = 1;

    /// <summary>
    /// Trả về Connect level index tương ứng.
    /// Hiện tại dùng mapping 1:1 (BirdSort level = Connect level).
    /// Có thể override logic tại đây nếu cần mapping khác.
    /// </summary>
    public static int GetConnectLevel() => BirdSortLevelOnEnterConnect;

    // ── Result ────────────────────────────────────────────────────────
    /// <summary>Kết quả Connect level: true = Win, false = Lose.</summary>
    public static bool LastConnectResult { get; set; } = false;
}
