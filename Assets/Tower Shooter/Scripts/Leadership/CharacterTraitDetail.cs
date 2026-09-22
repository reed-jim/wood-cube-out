using System.Collections.Generic;
using UnityEngine;

public class CharacterTrailDetail : MonoBehaviour
{
    public RectTransform container;

    private List<CharacterTraitItemUI> characterTraitItemUIs = new List<CharacterTraitItemUI>();

    public void Init(CharacterTrait characterTrait)
    {
        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        for (int i = 0; i < characterTrait.traitEfficiencyModifiers.Count; i++)
        {
            TraitEfficiencyModifier traitEfficiencyModifier = characterTrait.traitEfficiencyModifiers[i];

            CharacterTraitItemUI characterTraitItemUI = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(characterTraitItemUI.transform, container);

            characterTraitItemUI.Init(traitEfficiencyModifier);
        }
    }
}
