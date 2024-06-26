﻿using Mirror;
using System.Collections.Generic;
using System.Linq;

internal class NecroAttackAction : DefaultAttackAction, IPrintableStats
{
    const int NECRO_SELF_DAMAGE = 10;
    const DamageType NECRO_SELF_DAMAGE_TYPE = DamageType.magic;

    //Need this to grab attacks per turn
    //very ugly but hopefully can be removed once abilities are setup as SO that can define their own printouts
    const int NECRO_CLASS_ID = 7;

    public Dictionary<string, string> GetStatsDictionary()
    {
        Dictionary<string, string> toReturn = new();
        toReturn.Add("Self damage", Utility.DamageStatsToString(NECRO_SELF_DAMAGE, 1, NECRO_SELF_DAMAGE_TYPE));
        toReturn.Add("Attacks per turn", ClassDataSO.Singleton.GetClassByID(NECRO_CLASS_ID).stats.attacksPerTurn.ToString());
        return toReturn;
    }

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
                                              this.ActorCharacter.charClass.abilities.Single(ability => ability.stringID == "NecroMultiAttack").interfaceName,
                                              sender: this.RequestingClient);
    }

    [Server]
    public override ActionEffectPreview PreviewEffect() {
        ActionEffectPreview basePreview = base.PreviewEffect();
        ActionEffectPreview customPortionPreview = ActionExecutor.Singleton.GetCustomAttackPreview(source: this.ActorHex,
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
                                              this.DamageSourceName,
                                              sender: this.RequestingClient);
        
        return basePreview.MergeWithPreview(customPortionPreview);
    }
}