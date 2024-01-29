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

    public override bool ServerValidate()
    {
        if (!this.ActorHex.HoldsABallista())
        {
            Debug.Log("Ballista attack validation failed: Ballista attack cannot be performed at this location, no ballista here");
            return false;
        }
            
        if (this.ActorHex.BallistaNeedsReload())
        {
            Debug.Log("Ballista attack validation failed: Ballista cannot be used, needs reload");
            return false;
        }
            
        return base.ServerValidate();

    }
}
