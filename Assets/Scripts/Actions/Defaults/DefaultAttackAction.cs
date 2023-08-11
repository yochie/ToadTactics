using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class DefaultAttackAction : IAttackAction
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

    [Server]
    public void ServerUse()
    {

        if(this.TargetHex.HoldsAnObstacle() && this.DefenderCharacter == null)
        {
            //attacking obstacle
            GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            GameObject attackedTree = allObstacles.Where(obstacle => obstacle.GetComponent<Obstacle>().hexPosition.Equals(this.TargetHex.coordinates)).First();
            Object.Destroy(attackedTree);
            TargetHex.ClearObstacle();
            Debug.LogFormat("{0} attacked tree to destroy it", this.ActorCharacter);
        }
        else 
        {
            //attacking character
            float critChance = this.AttackerStats.critChance;
            bool penetrates = this.AttackerStats.penetratingDamage;

            for (int i = 0; i < this.AttackerStats.damageIterations; i++)
            {
                int prevLife = this.DefenderCharacter.CurrentLife;
                bool isCrit = Utility.RollCrit(critChance);
                int damageStatToUse = this.DefenderCharacter.IsKing ? this.AttackerStats.kingDamage : this.AttackerStats.damage;
                int critRolledDamage = isCrit ? Utility.CalculateCritDamage(damageStatToUse, this.AttackerStats.critMultiplier) : damageStatToUse;
                this.DefenderCharacter.TakeDamage(critRolledDamage, this.AttackerStats.damageType, penetrates);

                Debug.LogFormat("{0} hit {1} for {2} ({6} {5}{7}) leaving him with {3} => {4} life.",
                this.ActorCharacter,
                this.DefenderCharacter,
                critRolledDamage,
                prevLife,
                this.DefenderCharacter.CurrentLife,
                isCrit ? "crit" : "normal",
                this.AttackerStats.damageType,
                penetrates ? " penetrating" : "");

                if (this.DefenderCharacter.IsDead)
                    break;
            }
        }

        //PlayerCharacter state updated to track that attack was used
        this.ActorCharacter.UsedAttack();

        //TODO Move to event listeners
        //if(!this.ActorCharacter.HasAvailableAttacks())
        //    MainHUD.Singleton.TargetRpcGrayOutAttackButton(this.RequestingClient);

        //if (this.ActorCharacter.HasAvailableMoves())
        //{
        //    MapInputHandler.Singleton.TargetRpcSetControlMode(this.RequestingClient, ControlMode.move);
        //}
        //else
        //{
        //    MainHUD.Singleton.TargetRpcGrayOutMoveButton(this.RequestingClient);
        //    MapInputHandler.Singleton.TargetRpcSetControlMode(this.RequestingClient, ControlMode.none);
        //}

        //TODO: move to action executor FinishAction code to avoid recursion
        //if (!this.ActorCharacter.HasRemainingActions())
        //{
        //    GameController.Singleton.CmdNextTurn();
        //}
    }

    [Server]
    public bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            this.ActorCharacter.HasAvailableAttacks() &&
            ITargetedAction.ValidateTarget(this)
            )
            return true;
        else
            return false;
    }
}
