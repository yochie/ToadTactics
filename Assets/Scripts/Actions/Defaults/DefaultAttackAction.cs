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
    public void ServerUse(INetworkedLogger logger)
    {

        if(this.TargetHex.HoldsAnObstacle() && this.DefenderCharacter == null)
        {
            //attacking obstacle
            Utility.DestroyObstacleAt(this.TargetHex);

            string message = string.Format("{0} destroyed tree", this.ActorCharacter.charClass.name);
            logger.RpcLogMessage(message);
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

                string message = string.Format("{0} hit {1} for {2} ({6} {5}{7}) {3} => {4}",
                this.ActorCharacter.charClass.name,
                this.DefenderCharacter.charClass.name,
                critRolledDamage,
                prevLife,
                this.DefenderCharacter.CurrentLife,
                isCrit ? "crit" : "nocrit",
                this.AttackerStats.damageType,
                penetrates ? " penetrating" : "");

                logger.RpcLogMessage(message);

                if (this.DefenderCharacter.IsDead)
                    break;
            }
        }

        //PlayerCharacter state updated to track that attack was used
        this.ActorCharacter.UsedAttack();
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
