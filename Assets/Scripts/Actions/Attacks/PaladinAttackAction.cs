using Mirror;

internal class PaladinAttackAction : DefaultAttackAction
{
    const int PALADIN_FAITHLESS_BONUS_DAMAGE = 20;
    const DamageType PALADIN_FAITHLESS_BONUS_DAMAGE_TYPE = DamageType.magic;    

    [Server]
    public override void ServerUse(INetworkedLogger logger)
    {
        base.ServerUse(logger);
        if (!this.TargetHex.HoldsACharacter())
            return;
        PlayerCharacter defender = this.TargetHex.GetHeldCharacterObject();
        if (!defender.CurrentStats.hasFaith)
        {
            int prevLife = defender.CurrentLife;
            ActionExecutor.Singleton.CustomAttack(source: this.ActorHex,
                                              primaryTarget: this.TargetHex,
                                              areaType: AreaType.single,
                                              areaScaler: 1,
                                              damage: PALADIN_FAITHLESS_BONUS_DAMAGE,
                                              damageType: PALADIN_FAITHLESS_BONUS_DAMAGE_TYPE,
                                              damageIterations: 1,
                                              penetratingDamage: false,
                                              knockback: 0,
                                              canCrit: false,
                                              critChance: 0,
                                              critMultiplier: 0,
                                              sender: this.RequestingClient);
        }
    }

}