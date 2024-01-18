using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "StatIcon", menuName = "ScriptableObjects/StatIcons")]
public class StatIconSO : ScriptableObject
{
    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite => this.sprite;

    [SerializeField]
    private string statName;
    public string StatName => this.statName;
}
