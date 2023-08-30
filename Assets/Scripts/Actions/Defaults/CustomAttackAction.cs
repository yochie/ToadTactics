using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class CustomAttackAction : DefaultAttackAction
{
    //same as default but skip useAttack() call on character
    [Server]
    public override void ServerUse(INetworkedLogger logger)
    {
        List<Hex> allTargets = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);
        for (int i = 0; i < this.DamageIterations; i++)
        {
            foreach (Hex target in allTargets)
            {
                this.HitTarget(target, logger);
            }
        }
    }

    //Same as default but skip on checking if character has available attacks
    [Server]
    public override bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            ITargetedAction.ValidateTarget(this)
            )
            return true;
        else
            return false;
    }
}
