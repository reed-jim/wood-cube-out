using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTraitContainer", menuName = "Idle/Traits/CharacterTraitContainer")]
public class CharacterTraitContainer : ScriptableObject
{
    public List<CharacterTrait> characterTraits;
    public int maxTraitPerCharacter;

    public List<CharacterTrait> GetRandomTraits()
    {
        int n = Random.Range(1, maxTraitPerCharacter);

        List<CharacterTrait> randomTraits = characterTraits
            .ToList();

        for (int i = 0; i < n; i++)
        {
            int j = Random.Range(i, randomTraits.Count);
            (randomTraits[i], randomTraits[j]) = (randomTraits[j], randomTraits[i]);
        }

        randomTraits = randomTraits.GetRange(0, n);

        return randomTraits;
    }
}
