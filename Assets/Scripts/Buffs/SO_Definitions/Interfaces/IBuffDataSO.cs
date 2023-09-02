using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IBuffDataSO
{
    public string stringID { get; set; }

    public string UIName { get; set; }

    public bool IsPositive { get; set; }

    public DurationType DurationType { get; set; }

    public int TurnDuration{ get; set; }

    public Sprite Icon { get; set; }

    public string GetTooltipDescription();

    Dictionary<string, string> GetBuffStatsDictionary();

    public static string GetDurationDescritpion(IBuffDataSO buffData)
    {
        string durationDescription;
        switch (buffData.DurationType)
        {
            case DurationType.untilDeath:
                durationDescription = "until death";
                break;
            case DurationType.timed:
                durationDescription = string.Format("{0} turns", buffData.TurnDuration);
                break;
            case DurationType.eternal:
                durationDescription = string.Format("passive");
                break;
            case DurationType.conditional:
                IConditionalBuff conditionalBuff = buffData as IConditionalBuff;
                if (conditionalBuff == null)
                    throw new Exception("Buff has conditional duration but doesn't implement IConditionalBuff");
                durationDescription = string.Format("Lasts until {0}.", conditionalBuff.InlineConditionDescription);
                break;
            default:
                durationDescription = "An undefined amount of time...";
                break;
        }
        return durationDescription;
    }

}