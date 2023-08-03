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
    public void ServerUse()
    {

        if(this.TargetHex.holdsObstacle != ObstacleType.none && this.DefenderCharacter == null)
        {
            //attacking obstacle
            Debug.Log("Currently unsopported. Should not have passed validation.");
            //GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            //GameObject attackedTree = allObstacles.Where(obstacle => obstacle.GetComponent<Obstacle>().hexPosition.Equals(this.TargetHex.coordinates)).First();
            //Object.Destroy(attackedTree);
            //TargetHex.holdsObstacle = ObstacleType.none;
            //Debug.LogFormat("{0} attacked tree to destroy it", this.ActorCharacter);
        }
        else 
        {
            //attacking character
            float critChance = this.AttackerStats.critChance;           

            for (int i = 0; i < this.AbilityStats.damageIterations; i++)
            {
                int prevLife = this.DefenderCharacter.CurrentLife();
                bool isCrit = this.AbilityStats.canCrit ? Utility.RollCrit(critChance) : false;
                int rolledDamage = isCrit ? Utility.CalculateCritDamage(this.AbilityStats.damage, this.AbilityStats.damageIterations) : this.AbilityStats.damage;
                this.DefenderCharacter.TakeDamage(rolledDamage, this.AbilityStats.damageType);

                Debug.LogFormat("{0} hit {1} for {2} ({6} {5}) leaving him with {3} => {4} life.",
                this.ActorCharacter,
                this.DefenderCharacter,
                rolledDamage,
                prevLife,
                this.DefenderCharacter.CurrentLife(),
                isCrit ? "crit" : "normal",
                this.AbilityStats.damageType);

                if (this.DefenderCharacter.IsDead())
                {
                    Debug.LogFormat("{0} is dead.", this.DefenderCharacter);
                    this.TargetHex.ClearCharacter();
                    this.TargetHex.holdsCorpseWithClassID = DefenderCharacter.charClassID;
                    break;
                }
            }
        }
    }

    [Server]
    public bool ServerValidate()
    {
        if (this.ActorCharacter != null &&
            this.ActorHex != null &&            
            this.TargetHex != null &&
            this.TargetHex.HoldsACharacter() &&
            this.TargetHex.GetHeldCharacterObject() == this.DefenderCharacter &&
            this.RequestingPlayerID != -1 &&
            this.ActorHex.HoldsACharacter() &&
            this.ActorHex.GetHeldCharacterObject() == this.ActorCharacter &&
            this.RequestingPlayerID == this.ActorCharacter.ownerID &&
            GameController.Singleton.ItsThisPlayersTurn(this.RequestingPlayerID) &&
            GameController.Singleton.ItsThisCharactersTurn(this.ActorCharacter.charClassID) &&
            ITargetedAction.ValidateTarget(this)
            )
            return true;
        else
            return false;
    }
}
