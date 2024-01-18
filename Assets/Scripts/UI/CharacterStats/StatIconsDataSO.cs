using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class StatIconsDataSO : ScriptableObject
{
    [SerializeField]
    private List<StatIconSO> statIcons;

    public Sprite GetSpriteForStat(string statName)
    {
        return statIcons.Single((stat) => stat.StatName == statName).Sprite;
    }
}
