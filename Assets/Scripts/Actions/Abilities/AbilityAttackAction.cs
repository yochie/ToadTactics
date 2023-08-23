using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class AbilityAttackAction : IAttackAction, IAbilityAction
{
    //IAction
    public PlayerCharacter ActorCharacter { get; set; }
    public Hex ActorHex { get; set; }
    public int RequestingPlayerID { get; set; }
    public NetworkConnectionToClient RequestingClient { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    //IAttackAction
    public CharacterStats AttackerStats { get; set; }
    public CharacterStats DefenderStats { get; set; }
    public PlayerCharacter DefenderCharacter { get; set; }

    //IAbilityAction
    public CharacterAbilityStats AbilityStats { get; set; }

    [Server]
    public void ServerUse(INetworkedLogger logger)
    {

        if(this.TargetHex.HoldsAnObstacle() && this.DefenderCharacter == null)
        {
            //attacking obstacle
            GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            GameObject attackedTree = allObstacles.Where(obstacle => obstacle.GetComponent<Obstacle>().hexPosition.Equals(this.TargetHex.coordinates)).First();
            Object.Destroy(attackedTree);
            TargetHex.ClearObstacle();
            string message = string.Format("{0} destroyed tree", this.ActorCharacter.charClass.name);
            logger.RpcLogMessage(message);        
        }
        else 
        {
            //attacking character
            float critChance = this.AbilityStats.critChance == -1 ? this.AttackerStats.critChance : this.AbilityStats.critChance;
            float critMulti = this.AbilityStats.critMultiplier == -1 ? this.AttackerStats.critMultiplier : this.AbilityStats.critChance;
            bool penetrates = this.AbilityStats.penetratingDamage;

            for (int i = 0; i < this.AbilityStats.damageIterations; i++)
            {
                int prevLife = this.DefenderCharacter.CurrentLife;
                bool isCrit = this.AbilityStats.canCrit ? Utility.RollCrit(critChance) : false;
                int rolledDamage = isCrit ? Utility.CalculateCritDamage(this.AbilityStats.damage, critMulti) : this.AbilityStats.damage;
                this.DefenderCharacter.TakeDamage(rolledDamage, this.AbilityStats.damageType, penetrates);

                string message = string.Format("{0} hit {1} for {2} ({6} {5}{7}) {3} => {4}",
                this.ActorCharacter.charClass.name,
                this.DefenderCharacter.charClass.name,
                rolledDamage,
                prevLife,
                this.DefenderCharacter.CurrentLife,
                isCrit ? "crit" : "nocrit",
                this.AbilityStats.damageType,
                penetrates ? " penetrating" : "");

                logger.RpcLogMessage(message);

                if (this.DefenderCharacter.IsDead)
                {
                    break;
                }
            }
        }
    }

    [Server]
    public bool ServerValidate()
    {
        //if this is NOT an obstacle attack, make sure targets are correctly configured
        if (this.TargetHex.HoldsACharacter() && this.TargetHex.GetHeldCharacterObject() != this.DefenderCharacter)
            return false;

        if (IAction.ValidateBasicAction(this) &&
            ITargetedAction.ValidateTarget(this)
            )
            return true;
        else
            return false;
    }
}
