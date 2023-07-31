using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AbilitiesTable : MonoBehaviour
{

    [SerializeField]
    private GameObject abilityNameLabelPrefab;

    [SerializeField]
    private GameObject abilityDescriptionLabelPrefab;

    [SerializeField]
    private GameObject abilitiesNameTable;

    [SerializeField]
    private GameObject abilitiesDescriptionTable;

    public void RenderForClass(CharacterClass charClass)
    {
        var toPrint = this.GetPrintableAbilityDictionary(charClass);
        this.RenderFromDictionary(toPrint);
    }

    private Dictionary<string, string> GetPrintableAbilityDictionary(CharacterClass charClass)
    {
        Dictionary<string, string> toPrint = new();

        //return empty list if no abilities
        if (charClass.abilities == null)
            return toPrint;

        foreach (CharacterAbilityStats ability in charClass.abilities)
        {
            toPrint.Add(ability.interfaceName, ability.description);
        }

        return toPrint;
    }

    private void RenderFromDictionary(Dictionary<string, string> dictionary)
    {
        this.Clear();
        foreach(KeyValuePair<string, string> ability in dictionary)
        {
            GameObject nameLabelObject = Instantiate(this.abilityNameLabelPrefab, this.abilitiesNameTable.gameObject.transform);
            TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
            nameLabel.text = ability.Key;

            GameObject valueLabelObject = Instantiate(this.abilityDescriptionLabelPrefab, this.abilitiesDescriptionTable.gameObject.transform);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
            valueLabel.text = ability.Value;
        }
    }

    private void Clear()
    {
        foreach (TextMeshProUGUI child in this.abilitiesNameTable.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Destroy(child.gameObject);
        }

        foreach (TextMeshProUGUI child in this.abilitiesDescriptionTable.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Destroy(child.gameObject);
        }
    }
}
