using UnityEngine;

struct AbilityPrintData
{
    public readonly string name;
    public readonly string description;
    public readonly string damageOneLiner;
    public readonly string range;
    public readonly string aoe;
    public readonly string usesPerRound;
    public readonly string cooldownDuration;
    public readonly string passiveOrActive;
    public readonly Color damageColor;

    public readonly string currentCooldown;
    public readonly string currentRemainingUses;


    public AbilityPrintData(string name,
                            string description,
                            string passiveOrActive,
                            string damageOneLiner = "",
                            string range = "",
                            string aoe = "",
                            string usesPerRound = "",
                            string cooldownDuration = "",                       
                            string currentCooldown = "",
                            string currentRemainingUses = "",
                            Color? damageColor = null)
    {
        this.name = name;
        this.description = description;
        this.passiveOrActive = passiveOrActive;
        this.damageOneLiner = damageOneLiner;
        this.range = range;
        this.aoe = aoe;
        this.usesPerRound = usesPerRound;
        this.cooldownDuration = cooldownDuration;
        this.currentCooldown = currentCooldown;
        this.damageColor = damageColor == null ? Color.black : damageColor.GetValueOrDefault();
        this.currentRemainingUses = currentRemainingUses;
    }
}