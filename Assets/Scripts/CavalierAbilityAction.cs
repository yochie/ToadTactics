using Mirror;

public class CavalierAbilityAction : NetworkBehaviour, IAbilityAction
{
    public CharacterAbilityStats abilityStats { get; set; }

    public PlayerCharacter user { get; set; }

    public Hex target { get; set; }

    [Command]
    public void CmdUse()
    {
        //Action attack = new CustomAttackAction(user, target, abilityStats.damage, abilityStats.damageType, damageIterations: 1);
        //if (!attack.validate){ Debug.Log(); return;}
        //attack.CmdUse();

        //target.addBuff(new StunBuff(user, 1));
        throw new System.NotImplementedException();
    }

    public bool Validate()
    {
        throw new System.NotImplementedException();
    }
}