using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterTraitItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image circle;
    public TMP_Text traitName;
    public Button selectButton;

    public CharacterTrait characterTrait;

    [SerializeField] private float hoverDelay = 0.5f; // Set delay in seconds via Inspector
    private Coroutine hoverCoroutine;

    private string primaryHex;
    private string onSurfaceHex;

    public void Init(CharacterTrait characterTrait)
    {
        Color traitColor =
            characterTrait.isGoodTrait ? DynamicThemeManager.Ins.CurrentTheme.primary : DynamicThemeManager.Ins.CurrentTheme.error;

        circle.color = traitColor;
        traitName.color = traitColor;

        circle.gameObject.SetActive(true);

        traitName.text = characterTrait.traitName;

        this.characterTrait = characterTrait;
    }

    public void Init(TraitEfficiencyModifier traitEfficiencyModifier)
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);
        Color onSurfaceColor = DynamicThemeManager.Ins.CurrentTheme.onSurface;

        traitName.color = onSurfaceColor;

        circle.gameObject.SetActive(false);

        traitName.text =
            $"<color={primaryHex}>{traitEfficiencyModifier.efficiencyModifier * 100}% efficiency</color> for <color={primaryHex}>{traitEfficiencyModifier.leadershipPosition}</color> position";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop any existing coroutine to prevent overlap
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        // Start waiting
        hoverCoroutine = StartCoroutine(ShowDetailWithDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancel the delay if the pointer exits early
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        // Hide the detail view
        LeadershipManager.Ins.HideTraitDetail();
    }

    private IEnumerator ShowDetailWithDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        LeadershipManager.Ins.ShowTraitDetail(this);
        hoverCoroutine = null;
    }
}
