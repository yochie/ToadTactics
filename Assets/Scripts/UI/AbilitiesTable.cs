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
