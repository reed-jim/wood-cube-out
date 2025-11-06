using System.Collections.Generic;
using NamPhuThuy;
using NamPhuThuy.Data;
using UnityEngine;

public interface IPictureProgress
{
    bool IsUnlocked(int pictureId);
    void MarkUnlocked(IEnumerable<int> pictureIds);
}

public interface IPictureRepository
{
    // Queries
    List<PictureData> GetLockedByStyle(PictureData.PictureStyle style);
    List<PictureData> DrawRandomLockedByStyle(PictureData.PictureStyle style, int count, int? seed = null);
}

public class PictureRepository : IPictureRepository
{
    private readonly PictureDatas _db;
    private readonly IPictureProgress _progress;

    public PictureRepository(PictureDatas db, IPictureProgress progress)
    {
        _db = db;
        _progress = progress;
    }

    public List<PictureData> GetLockedByStyle(PictureData.PictureStyle style)
    {
        var list = new List<PictureData>();
        foreach (var p in _db.allPictureDatas)
        {
            if (p.style == style && !_progress.IsUnlocked(p.picId)) // <-- use an ID
                list.Add(p);
        }
        return list;
    }

    public List<PictureData> DrawRandomLockedByStyle(PictureData.PictureStyle style, int count, int? seed = null)
    {
        var pool = GetLockedByStyle(style);
        if (pool.Count == 0) return pool;

        // Deterministic shuffle if a seed is provided (useful for tests)
        var rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int take = Mathf.Min(count, pool.Count);
        return pool.GetRange(0, take);
    }
}