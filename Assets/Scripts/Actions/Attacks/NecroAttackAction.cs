using Mirror;

internal class NecroAttackAction : DefaultAttackAction
{
    const int NECRO_SELF_DAMAGE = 10;
    const DamageType NECRO_SELF_DAMAGE_TYPE = DamageType.magic;

    [Server]
    public override void ServerUse(INetworkedLogger logger)
    {
        base.ServerUse(logger);
        ActionExecutor.Singleton.CustomAttack(source: this.ActorHex,
                                              primaryTarget: this.ActorHex,
                                              areaType: AreaType.single,
                                              areaScaler: 1,
                                              damage: NECRO_SELF_DAMAGE,
                                              damageType: NECRO_SELF_DAMAGE_TYPE,
                                              damageIterations: 1,
                                              penetratingDamage: false,
                                              knockback: 0,
                                              canCrit: false,
                                              critChance: 0,
                                              critMultiplier: 0, 
                                              sender: this.RequestingClient);
    }

}