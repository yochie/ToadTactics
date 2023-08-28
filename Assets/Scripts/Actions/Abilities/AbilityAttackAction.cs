using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class AbilityAttackAction : DefaultAttackAction
{

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
