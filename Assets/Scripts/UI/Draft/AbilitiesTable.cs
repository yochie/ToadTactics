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

    internal void RenderFromDictionary(Dictionary<string, string> dictionary)
    {
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
}
