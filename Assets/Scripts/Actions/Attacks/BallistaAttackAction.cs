using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class BallistaAttackAction : CustomAttackAction
{
    //same as any custom attack but mark character as having used ballista action
    [Server]
    public override void ServerUse(INetworkedLogger logger)
    {
        base.ServerUse(logger);
        this.ActorCharacter.UsedBallista();
        ActorHex.UseBallista();
    }
}
