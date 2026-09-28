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

    public List<CharacterTrait> GetRandomTraits(float goodTraitChance)
    {
        // Determine total number of traits to select
        // Note: Random.Range(int minInclusive, int maxExclusive) needs maxTraitPerCharacter + 1 to include maxTraitPerCharacter
        int count = Random.Range(1, maxTraitPerCharacter + 1);

        // Separate available traits into good and bad/other lists
        List<CharacterTrait> goodTraits = characterTraits.Where(t => t.isGoodTrait).ToList();
        List<CharacterTrait> otherTraits = characterTraits.Where(t => !t.isGoodTrait).ToList();

        List<CharacterTrait> selectedTraits = new List<CharacterTrait>();

        for (int i = 0; i < count; i++)
        {
            // Decide whether to roll for a good trait based on input percentage
            bool selectGood = Random.Range(0f, 1) < goodTraitChance;

            // Fallback checks: if the preferred pool is empty, use the other pool
            if (selectGood && goodTraits.Count == 0) selectGood = false;
            if (!selectGood && otherTraits.Count == 0) selectGood = true;

            // If both pools are completely empty, break early
            if (goodTraits.Count == 0 && otherTraits.Count == 0) break;

            List<CharacterTrait> targetPool = selectGood ? goodTraits : otherTraits;

            // Pick a random trait from the chosen pool
            int randomIndex = Random.Range(0, targetPool.Count);
            selectedTraits.Add(targetPool[randomIndex]);

            // Remove selected trait to prevent duplicate selections
            targetPool.RemoveAt(randomIndex);
        }

        return selectedTraits;
    }
}
