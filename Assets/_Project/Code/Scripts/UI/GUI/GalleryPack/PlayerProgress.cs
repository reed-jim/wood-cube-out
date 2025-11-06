using System.Collections.Generic;

public class PlayerProgress : IPictureProgress
{
    private readonly HashSet<int> _unlocked = new HashSet<int>();
    public bool IsUnlocked(int pictureId) => _unlocked.Contains(pictureId);
    public void MarkUnlocked(IEnumerable<int> ids) { foreach (var id in ids) _unlocked.Add(id); }
}