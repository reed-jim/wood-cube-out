using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/IntVariable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int value;
    [SerializeField] private bool isSave;
    [SerializeField] private string saveKey;

    private bool IsCurrentLevelKey()
    {
        return isSave && !string.IsNullOrEmpty(saveKey) && 
               (saveKey == "Current Level" || 
                saveKey == "current_level" || 
                saveKey == "currentLevel" || 
                saveKey == "CURRENT_LEVEL");
    }

    public int Value
    {
        get
        {
            if (IsCurrentLevelKey())
            {
                return BirdSort2.UseProfile.CurrentLevel;
            }
            return value;
        }
        set
        {
            this.value = value;
            if (IsCurrentLevelKey())
            {
                BirdSort2.UseProfile.CurrentLevel = value;
                if (ConnectBridge.IsLaunchedFromBirdSort)
                {
                    ConnectBridge.BirdSortLevelOnEnterConnect = value;
                }
            }
        }
    }

    public int ValueUsingPureJson
    {
        get
        {
            if (IsCurrentLevelKey())
            {
                return BirdSort2.UseProfile.CurrentLevel;
            }
            return DataUtility.Load(saveKey, 1);
        }
        set
        {
            if (IsCurrentLevelKey())
            {
                BirdSort2.UseProfile.CurrentLevel = value;
            }
            else
            {
                DataUtility.Save(saveKey, value);
            }
        }
    }

    public void Save(string key)
    {
        if (IsCurrentLevelKey() && (key == "Current Level" || key == "current_level" || key == "currentLevel" || key == "CURRENT_LEVEL"))
        {
            BirdSort2.UseProfile.CurrentLevel = value;
        }
        else
        {
            DataUtility.SaveAsync(key, value);
        }
    }

    public void Save()
    {
        if (IsCurrentLevelKey())
        {
            BirdSort2.UseProfile.CurrentLevel = value;
        }
        else
        {
            DataUtility.SaveAsync(saveKey, value);
        }
    }

    public async Task SaveAsync()
    {
        if (IsCurrentLevelKey())
        {
            BirdSort2.UseProfile.CurrentLevel = value;
        }
        else
        {
            await DataUtility.SaveAsync(saveKey, value);
        }
    }

    public void Load()
    {
        if (IsCurrentLevelKey())
        {
            value = BirdSort2.UseProfile.CurrentLevel;
        }
        else
        {
            value = DataUtility.Load(saveKey, 1);
        }
    }
}
