using Mirror;
using UnityEngine;

public class CombatManager : NetworkBehaviour
{
    [Server]
    public static void Attack(PlayerCharacter attacker, PlayerCharacter defender)
    {
        int prevLife = defender.CurrentLife();
        for (int i = 0; i < attacker.currentStats.damageIterations; i++)
        {
            switch(attacker.currentStats.damageType) {
                case DamageType.normal:
                    defender.TakeRawDamage(attacker.currentStats.damage - defender.currentStats.armor);
                    break;
                case DamageType.magic:
                    defender.TakeRawDamage(attacker.currentStats.damage);
                    break;
                case DamageType.healing:
                    defender.TakeRawDamage(-attacker.currentStats.damage);
                    break;
            }           
        }       

        //use PlayerCharacter attack action
        attacker.UseAttack();

        Debug.LogFormat("{0} has attacked {1} for {2}x{3} leaving him with {4} => {5} life.", attacker, defender, attacker.currentStats.damage, attacker.currentStats.damageIterations, prevLife, defender.CurrentLife());
    }
}