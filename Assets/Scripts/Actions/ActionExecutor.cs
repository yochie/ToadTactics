using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class ActionExecutor : NetworkBehaviour
{
    private INetworkedLogger logger;

    [SerializeField]
    private IntIntGameEventSO OnCharacterAttacksServerSide;

    public static ActionExecutor Singleton { get; private set; }

    public void Awake()
    {
        ActionExecutor.Singleton = this;
        this.logger = MasterLogger.Singleton;
    }

    [Command(requiresAuthority = false)]
    public void CmdMoveChar(Hex source, Hex dest, NetworkConnectionToClient sender = null)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter movingCharacter = source.GetHeldCharacterObject();
        IMoveAction moveAction = ActionFactory.CreateMoveAction(sender, playerID, movingCharacter, movingCharacter.CurrentStats, source, dest);
        moveAction.SetupPath();
        this.TryAction(moveAction, isFullAction: true, startingMode: ControlMode.move);
    }

    internal bool CustomMove(Hex source,
                           Hex dest,              
                           NetworkConnectionToClient sender)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter movingCharacter = source.GetHeldCharacterObject();
        CustomMoveAction moveAction = ActionFactory.CreateCustomMoveAction(sender, playerID, movingCharacter, source, dest);
        moveAction.SetupPath();
        bool success = this.TryAction(moveAction, isFullAction: false, startingMode: ControlMode.move);
        return success;
    }


    [Command(requiresAuthority = false)]
    public void CmdAttack(Hex source, Hex target, NetworkConnectionToClient sender = null)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter attackingCharacter = source.GetHeldCharacterObject();
        IAttackAction attackAction = ActionFactory.CreateAttackAction(sender, playerID, attackingCharacter, attackingCharacter.CurrentStats, source, target);
        this.TryAction(attackAction, isFullAction: true, startingMode: ControlMode.attack);

        int attackedCharacterId = -1;
        if (target.HoldsACharacter())
            attackedCharacterId = target.GetHeldCharacterObject().CharClassID;
        OnCharacterAttacksServerSide.Raise(attackingCharacter.CharClassID, attackedCharacterId);
    }



    [Command(requiresAuthority = false)]
    internal void CmdUseAbility(Hex source, Hex target, CharacterAbilityStats abilityStats, NetworkConnectionToClient sender = null)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter usingCharacter = source.GetHeldCharacterObject();
        IAbilityAction abilityAction = ActionFactory.CreateAbilityAction(sender, playerID, usingCharacter, abilityStats, source, target);
        this.TryAction(abilityAction, isFullAction: true, startingMode: ControlMode.useAbility);
    }

    //should only be used from abilities to handle their attack portions
    //since called within another action, dont call FinishAction(), parent action will take care of that
    [Server]
    public void AbilityAttack(Hex source, Hex target, CharacterAbilityStats abilityStats, NetworkConnectionToClient sender)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter attackingCharacter = source.GetHeldCharacterObject();
        CustomAttackAction abilityAttackAction = ActionFactory.CreateAbilityAttackAction(sender,
                                                                                   playerID,
                                                                                   attackingCharacter,
                                                                                   attackingCharacter.CurrentStats,
                                                                                   abilityStats,
                                                                                   source,
                                                                                   target);
        this.TryAction(abilityAttackAction, isFullAction : false);

        int attackedCharacterId = -1;
        if (target.HoldsACharacter())
            attackedCharacterId = target.GetHeldCharacterObject().CharClassID;
        OnCharacterAttacksServerSide.Raise(attackingCharacter.CharClassID, attackedCharacterId);
    }

    internal bool CustomAttack(Hex source,
                               Hex primaryTarget,
                               AreaType areaType,
                               int areaScaler,
                               int damage,
                               DamageType damageType,
                               int damageIterations,
                               bool penetratingDamage,
                               int knockback,
                               bool canCrit,
                               float critChance,
                               float critMultiplier,
                               NetworkConnectionToClient sender)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter attackingCharacter = source.GetHeldCharacterObject();
        CustomAttackAction customAttackAction = ActionFactory.CreateCustomAttackAction(
                                                                 sender,
                                                                 playerID,
                                                                 attackingCharacter,
                                                                 damage,
                                                                 damageType,
                                                                 damageIterations,
                                                                 penetratingDamage,
                                                                 knockback,
                                                                 canCrit,
                                                                 critChance,
                                                                 critMultiplier,
                                                                 areaType,
                                                                 areaScaler,
                                                                 source,
                                                                 primaryTarget);
        bool success = this.TryAction(customAttackAction, isFullAction: false);
        return success;
    }

    [Server]
    private bool TryAction(IAction action, bool isFullAction, ControlMode? startingMode = null)
    {
        bool success;
        if (action.ServerValidate())
        {
            action.ServerUse(this.logger);            
            success = true;
        }
        else
        {
            Debug.LogFormat("Action {0} validation failed.", action);
            success = false;
        }

        if (success && isFullAction && startingMode != null)
        {
            this.FinishAction(action.ActorCharacter, action.RequestingClient, startingMode.GetValueOrDefault());
        }

        return success;
    }


    //Updates input state and game progress resulting from action execution
    [Server]
    private void FinishAction(PlayerCharacter actor, NetworkConnectionToClient sender, ControlMode currentControlMode)
    {
        foreach(PlayerCharacter playerCharacter in GameController.Singleton.PlayerCharactersByID.Values)
        {
            //not necessarily changed but always call just in case
            playerCharacter.RpcOnCharacterLifeChanged(playerCharacter.CurrentLife, playerCharacter.CurrentStats.maxHealth);

            //check for end of round
            if (playerCharacter.IsKing && playerCharacter.IsDead)
            {
                Debug.Log("The king is dead. Long live the king.");
                //TODO : set flag to end round instead of calling function to avoid recursion
                GameController.Singleton.EndRound(playerCharacter.OwnerID);
                return;
            }
        }

        if (!actor.HasRemainingActions() || actor.IsDead)
        {
            //TODO : set flag to end turn instead of calling function to avoid recursion
            GameController.Singleton.CmdNextTurn();
            return;
        }
        else
        {
            List<ControlMode> activeControlModes = actor.GetRemainingActions();            
            if (!activeControlModes.Contains(currentControlMode))
            {
                //switch to next available control mode
                MainHUD.Singleton.TargetRpcToggleActiveButtons(sender, activeControlModes, activeControlModes[0]);
                MapInputHandler.Singleton.TargetRpcSetControlMode(sender, activeControlModes[0]);
            }
            else
                MainHUD.Singleton.TargetRpcToggleActiveButtons(sender, activeControlModes, currentControlMode);

            if (actor.HasActiveAbility())
            {
                string abilityID = actor.charClass.abilities[0].stringID;
                MainHUD.Singleton.TargetRpcUpdateAbilityCooldownIndicator(sender, actor.GetAbilityCooldown(abilityID), actor.GetAbilityUsesRemaining(abilityID));
            }

            //in case action caused character move
            Hex actorHex = Map.GetHex(Map.Singleton.hexGrid, Map.Singleton.characterPositions[actor.CharClassID]);
            MapInputHandler.Singleton.TargetRpcSelectHex(sender,actorHex);
        }
    }

    //Utility for validation of ITargetedActions and to find attack range in MapPathFinder
    public static bool IsValidTargetType(PlayerCharacter actor, Hex targetedHex, List<TargetType> allowedTargets)
    {
        bool selfTarget = false;
        bool friendlyTarget = false;
        bool ennemyTarget = false;
        bool untargetable = false;
        if (targetedHex.HoldsACharacter() || targetedHex.HoldsACorpse())
        {
            PlayerCharacter targetedCharacter = targetedHex.HoldsACharacter() ? targetedHex.GetHeldCharacterObject() : targetedHex.GetHeldCorpseCharacterObject();
            selfTarget = (actor.CharClassID == targetedCharacter.CharClassID);
            friendlyTarget = (actor.OwnerID == targetedCharacter.OwnerID);
            ennemyTarget = !friendlyTarget;
            untargetable = targetedCharacter.CurrentStats.isStealthy;
        }
        bool liveTarget = targetedHex.HoldsACharacter();
        bool corpseTarget = targetedHex.HoldsACorpse();
        bool emptyTarget = !targetedHex.HoldsACharacter() && !targetedHex.HoldsACorpse() && !targetedHex.HoldsAnObstacle();
        bool obstacleTarget = targetedHex.HoldsAnObstacle();

        if (!allowedTargets.Contains(TargetType.untargetable_ennemy) && (ennemyTarget && liveTarget && untargetable))
            return false;

        if (!allowedTargets.Contains(TargetType.untargetable_ally) && (friendlyTarget && liveTarget && untargetable))
            return false;

        if (!allowedTargets.Contains(TargetType.ennemy_chars) && (ennemyTarget && liveTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.other_friendly_chars) && (friendlyTarget && !selfTarget && liveTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.self) && (selfTarget && liveTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.empty_hex) && emptyTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.obstacle) && obstacleTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.ennemy_corpse) && (ennemyTarget && corpseTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.friendly_corpse) && (friendlyTarget && corpseTarget))
            return false;

        return true;
    }
}
