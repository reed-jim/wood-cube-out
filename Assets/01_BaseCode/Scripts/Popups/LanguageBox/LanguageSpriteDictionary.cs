using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/LanguageDatabase", fileName = "LanguageDatabase.asset")]
public class LanguageSpriteDictionary : SerializedScriptableObject
{
    public List<SystemLanguage> Value;
}
