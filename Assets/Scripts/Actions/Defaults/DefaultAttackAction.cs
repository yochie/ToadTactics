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

    public NetworkConnectionToClient Sender { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }

    //IAttackAction
    public CharacterStats AttackerStats { get; set; }
    public CharacterStats DefenderStats { get; set; }
    public PlayerCharacter DefenderCharacter { get; set; }

    [Server]
    public void ServerUse()
    {

        if(this.TargetHex.holdsObstacle != ObstacleType.none && this.DefenderCharacter == null)
        {
            //attacking obstacle
            GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            GameObject attackedTree = allObstacles.Where(obstacle => obstacle.GetComponent<Obstacle>().hexPosition.Equals(this.TargetHex.coordinates)).First();
            Object.Destroy(attackedTree);
            TargetHex.holdsObstacle = ObstacleType.none;            
        } else 
        {
            //attacking character
            int prevLife = this.DefenderCharacter.CurrentLife();
            for (int i = 0; i < this.AttackerStats.damageIterations; i++)
            {
                switch (this.AttackerStats.damageType)
                {
                    case DamageType.normal:
                        this.DefenderCharacter.TakeRawDamage(this.AttackerStats.damage - this.DefenderStats.armor);
                        break;
                    case DamageType.magic:
                        this.DefenderCharacter.TakeRawDamage(this.AttackerStats.damage);
                        break;
                    case DamageType.healing:
                        this.DefenderCharacter.TakeRawDamage(-this.AttackerStats.damage);
                        break;
                }
            }

            Debug.LogFormat("{0} has attacked {1} for {2}x{3} leaving him with {4} => {5} life.",
                            this.ActorCharacter,
                            this.DefenderCharacter,
                            this.AttackerStats.damage,
                            this.AttackerStats.damageIterations,
                            prevLife,
                            this.DefenderCharacter.CurrentLife());
        }

        //PlayerCharacter state updated to track that attack was used
        this.ActorCharacter.UseAttack();

        GameController.Singleton.RpcGrayOutAttackButton(this.Sender);

        if (this.ActorCharacter.CanMoveDistance() > 0)
        {
            GameController.Singleton.RpcSetControlModeOnClient(this.Sender, ControlMode.move);
        }
        else
        {
            GameController.Singleton.RpcGrayOutMoveButton(this.Sender);
            GameController.Singleton.RpcSetControlModeOnClient(this.Sender, ControlMode.none);
        }

        if (!this.ActorCharacter.HasRemainingActions())
        {
            GameController.Singleton.NextTurn();
        }
    }

    [Server]
    public bool ServerValidate()
    {
        if (this.ActorCharacter != null &&
            this.ActorHex != null &&            
            this.TargetHex != null &&
            this.RequestingPlayerID != -1 &&
            this.ActorHex.HoldsACharacter() &&
            this.ActorHex.GetHeldCharacterObject() == this.ActorCharacter &&
            MapPathfinder.LOSReaches(this.ActorHex, this.TargetHex, this.AttackerStats.range) &&
            !this.ActorCharacter.hasAttacked &&
            this.RequestingPlayerID == this.ActorCharacter.ownerID &&
            GameController.Singleton.IsItThisPlayersTurn(this.RequestingPlayerID) &&
            GameController.Singleton.IsItThisCharactersTurn(this.ActorCharacter.charClassID) &&
            ActionExecutor.IsValidTargetType(this.ActorCharacter, this.TargetHex, this.AttackerStats.allowedAttackTargets)
            )
            return true;
        else
            return false;
    }
}
