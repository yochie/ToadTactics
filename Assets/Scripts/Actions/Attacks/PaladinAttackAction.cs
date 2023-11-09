using Mirror;
using System.Collections.Generic;

internal class PaladinAttackAction : DefaultAttackAction, IPrintableStats
{
    static readonly int PALADIN_FAITHLESS_BONUS_DAMAGE = 20;
    static readonly DamageType PALADIN_FAITHLESS_BONUS_DAMAGE_TYPE = DamageType.magic;
    
    public Dictionary<string, string> GetStatsDictionary()
    {
        var toReturn = new Dictionary<string, string>();
        toReturn.Add("Bonus damage vs faithless", Utility.DamageStatsToString(PALADIN_FAITHLESS_BONUS_DAMAGE, 1, PALADIN_FAITHLESS_BONUS_DAMAGE_TYPE));
        return toReturn;
    }

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

    [Server]
    public override ActionEffectPreview PreviewEffect()
    {
        ActionEffectPreview basePreview = base.PreviewEffect();
        PlayerCharacter defender = this.TargetHex.GetHeldCharacterObject();
        if (!defender.CurrentStats.hasFaith)
        {
            ActionEffectPreview customPortionPreview = ActionExecutor.Singleton.GetCustomAttackPreview(source: this.ActorHex,
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

            return basePreview.MergeWithPreview(customPortionPreview);
        } else
        {
            return basePreview;
        }

    }

}