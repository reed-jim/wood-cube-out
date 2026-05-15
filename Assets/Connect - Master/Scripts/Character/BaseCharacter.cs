using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public CharacterServiceLocator characterServiceLocator;

    public TMP_Text[] stackingTexts;

    public List<InventoryItem> inventoryItems;
    public List<DamageInflicter> damageInflicters;

    void Update()
    {
        ApplyDamage();
    }

    void OnCollisionEnter(Collision collision)
    {
        DamageInflicter damageInflicter = collision.gameObject.GetComponent<DamageInflicter>();

        damageInflicters.Add(damageInflicter);
    }

    public void EquipInventoryItem(InventoryItem inventoryItem)
    {
        inventoryItems.Add(inventoryItem);

        DataUtility.Save(gameObject.name, this);
    }

    private void ApplyDamage()
    {
        float currentTime = Time.time;

        for (int i = 0; i < damageInflicters.Count; i++)
        {
            DamageInflicter damageInflicter = damageInflicters[i];

            if (damageInflicter.IsNewCycleToApplyDamage(currentTime))
            {
                characterServiceLocator.characterStat.health.currentValue -= damageInflicter.damagePerCycle;

                damageInflicter.UpdateNewDamageCycle(currentTime);
            }
        }

        damageInflicters = damageInflicters.Where(item => !item.IsExpire()).ToList();
    }

    private float CalculateFinalDamage(float flatDamage, ArmorPenetration armorPenetration)
    {
        float effectiveArmor = (characterServiceLocator.characterStat.armor.currentValue - armorPenetration.flatAmount) * (1 - armorPenetration.percent);
        float finalDamage = flatDamage * (100 / (100 + effectiveArmor));

        return finalDamage;
    }

    private void ShowDamageStatus()
    {
        Dictionary<DamageType, int> stackingPerDamageType = new Dictionary<DamageType, int>();

        for (int i = 0; i < damageInflicters.Count; i++)
        {
            DamageInflicter damageInflicter = damageInflicters[i];

            CommonUtil.SumValueOfDictionary(stackingPerDamageType, damageInflicter.damageType, 1);
        }

        for (int i = 0; i < stackingTexts.Length; i++)
        {
            stackingTexts[i].text = $"{stackingPerDamageType.ElementAt(i)}";
        }
    }
}
