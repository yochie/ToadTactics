using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruidMoveAction : DefaultMoveAction
{
    public override void ServerUse(INetworkedLogger logger)
    {
        base.ServerUse(logger);
        List<Hex> fullMovePath = new();
        fullMovePath.Add(this.ActorHex);
        fullMovePath.AddRange(this.movePath);
        foreach(Hex hexWalkedOn in fullMovePath)
        {
            if (hexWalkedOn == this.interruptedAtHex || hexWalkedOn == this.TargetHex)
                return;
            if (hexWalkedOn.IsEmpty())
            {
                Map.Singleton.obstacleManager.SpawnObstacleOnMap(Map.Singleton.hexGrid, hexWalkedOn.coordinates.OffsetCoordinatesAsVector(), ObstacleType.tree);
            }
        }
    }
}
