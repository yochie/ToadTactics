using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameplayPhase : IGamePhase
{
    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.gameplay;

    public GameController Controller { get; set; }

    [Server]
    public void Init(string name, GameController controller)
    {
        Debug.Log("Initializing gameplay mode");

        this.Name = name;
        this.Controller = controller;
        foreach (PlayerController player in Controller.playerControllers)
        {
            MainHUD.Singleton.RpcDisablePlacementInstruction();
        }
        //TODO : fix bug here caused by race condition : need to either wait for this to resolve on all clients OR integrate to initial character selection.....
        this.Controller.LocalPlayer.RpcClearStartZones();

        GameController.Singleton.RpcLoopGameplaySongs();

        this.Controller.StartCoroutine(this.CoroutineWaitForStartZoneClear());
    }

    private void FinishInit()
    {
        Controller.SetTurnOrderIndex(0);
        Controller.SetPlayerTurn(-1);

        //finds character class id for the next turn so that we can check who owns it
        int currentCharacterClassID = this.Controller.GetCharacterIDForTurn();
        PlayerCharacter currentCharacter = this.Controller.PlayerCharactersByID[currentCharacterClassID];

        //if we don't own that char, swap player turn
        //if (Controller.PlayerTurn != currentCharacter.OwnerID)
        //{
        //    Controller.SwapPlayerTurn();
        //}
        Controller.SetPlayerTurn(currentCharacter.OwnerID);

        this.SetupControlModes(currentCharacter);

        Controller.RpcOnInitGameplayMode();
    }

    [Server]
    public void Tick()
    {
        this.EndOfTurn();

        this.IncrementTurnOrder();

        this.StartOfTurn();
    }
    private void EndOfTurn()
    {
        int lastTurnCharacterID = this.Controller.GetCharacterIDForTurn();
        if (lastTurnCharacterID == -1)
        {
            throw new System.Exception("Error : couldn't find playing character in turn order");
        }
        PlayerCharacter lastTurnCharacter = this.Controller.PlayerCharactersByID[lastTurnCharacterID];

        BuffManager.Singleton.TickBuffsForTurn(lastTurnCharacterID);

        //need to check for king death after ticking buffs
        //need to check ALL kings since losing a buff might also kill you
        int deadKingOwnerID = this.Controller.AKingIsDead();
        if (deadKingOwnerID >= 0)
        {           
            Debug.Log("The king is dead. Long live the king.");
            GameController.Singleton.EndRound(looserID: deadKingOwnerID);
            return;
        }

        lastTurnCharacter.TickCooldownsForTurn();

        this.ApplyHazardDamageForTurnEnd(lastTurnCharacter);

        if(lastTurnCharacter.IsDead && lastTurnCharacter.IsKing)
        {
            Debug.Log("The king is dead. Long live the king.");
            GameController.Singleton.EndRound(looserID: lastTurnCharacter.OwnerID);
            return;
        }

        //update life on all chars because it might have been changed by buffs or hazards
        //execution order of RPCs is undetermined but should not matter since they are independent from eachother
        foreach(PlayerCharacter character in GameController.Singleton.PlayerCharactersByID.Values)
        {
            character.RpcOnCharacterLifeChanged(character.CurrentLife, character.CurrentStats.maxHealth);
        }
    }

    private void ApplyHazardDamageForTurnEnd(PlayerCharacter lastTurnCharacter)
    {
        if (lastTurnCharacter.IsDead)
            return;

        HazardType standingOnHazardType = Map.Singleton.IsCharacterStandingOnHazard(lastTurnCharacter.CharClassID);
        if (standingOnHazardType != HazardType.none)
        {
            int damageTaken = HazardDataSO.Singleton.GetHazardDamage(standingOnHazardType, standingDamage: true);
            DamageType damageTypeTaken = HazardDataSO.Singleton.GetHazardDamageType(standingOnHazardType);
            if (damageTypeTaken == DamageType.none || damageTaken == 0)
                return;
            lastTurnCharacter.TakeDamage(new Hit(damageTaken, damageTypeTaken, HitSource.FireHazard));
            if (damageTaken > 0)
            {
                string message = string.Format("{0} takes {1} {2} damage at end of turn for standing on {3} hazard",
                    lastTurnCharacter.charClass.name,
                    damageTaken,
                    damageTypeTaken,
                    standingOnHazardType);
                MasterLogger.Singleton.RpcLogMessage(message);
            }
        }
    }

    private void StartOfTurn()
    {
        //finds character class id for the next turn so that we can check who owns it        
        int newTurnCharacterID = this.Controller.GetCharacterIDForTurn();
        if (newTurnCharacterID == -1)
        {
            throw new System.Exception("Error : couldn't find playing character in turn order");
        }

        PlayerCharacter currentCharacter = this.Controller.PlayerCharactersByID[newTurnCharacterID];

        if (currentCharacter.IsDead || !currentCharacter.CanTakeTurns)
        {
            //skips turn
            this.Controller.CmdNextTurn();
            return;
        }

        currentCharacter.ResetTurnState();

        //if we don't own that char, swap player turn
        if (this.Controller.PlayerTurn != this.Controller.DraftedCharacterOwners[newTurnCharacterID])
        {
            this.Controller.SwapPlayerTurn();
        }

        this.SetupControlModes(currentCharacter);
    }

    private void IncrementTurnOrder()
    {
        if (this.Controller.TurnOrderIndex >= this.Controller.SortedTurnOrder.Count - 1)
            this.Controller.SetTurnOrderIndex(0);
        else
            this.Controller.SetTurnOrderIndex(this.Controller.TurnOrderIndex + 1);
    }

    private void SetupControlModes(PlayerCharacter currentCharacter)
    {
        List<ControlMode> activeControlModes = currentCharacter.GetRemainingActions();

        ControlMode startingMode = activeControlModes.Contains(ControlMode.move) ? ControlMode.move : ControlMode.none;

        this.Controller.AssignControlModesForNewTurn(this.Controller.PlayerTurn, startingMode, currentCharacter.CharClassID);

        NetworkConnectionToClient client = this.Controller.GetConnectionForPlayerID(currentCharacter.OwnerID);

        bool characterOnBallista = Map.Singleton.IsCharacterOnBallista(currentCharacter.CharClassID);

        //TODO: fix to better determine whether a character has active abilities
        //right now it just assumes that any active ability will be the first one listed... pretty horrible
        CharacterAbilityStats abilityStats = currentCharacter.charClass.abilities[0];
        if (abilityStats.isPassive)
        {
            //passing in empty strings since RPC cannot have optional arguments
            MainHUD.Singleton.TargetRpcSetupButtonsForTurn(target: client, interactableButtons: activeControlModes, toHighlight: startingMode, abilityName: "", abilityCooldown: -1, usesRemaining: -1, hasActiveAbility:false, onBallista: characterOnBallista);
        }
        else
        {
            string abilityName = abilityStats.interfaceName;
            string abilityID = currentCharacter.charClass.abilities[0].stringID;
            int abilityCooldown = currentCharacter.GetAbilityCooldown(abilityID);
            int remainingUses = currentCharacter.GetAbilityUsesRemaining(abilityID);

            MainHUD.Singleton.TargetRpcSetupButtonsForTurn(target: client, activeControlModes, startingMode, abilityName, abilityCooldown, remainingUses, hasActiveAbility: true, onBallista: characterOnBallista);
        }
    }

    private IEnumerator CoroutineWaitForStartZoneClear() {
        while (!this.Controller.StartZonesCleared)
        {
            yield return null;
        }

        this.FinishInit();
    }
}
