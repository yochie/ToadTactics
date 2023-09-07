using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruidMoveAction : DefaultMoveAction, IPrintableStats
{
    private static readonly ObstacleType SPAWNED_OBSTACLE_TYPE = ObstacleType.tree;

    public Dictionary<string, string> GetStatsDictionary()
    {
        return new() { { "Spawned obstacle", string.Format("{0}", SPAWNED_OBSTACLE_TYPE) } };
    }

    public override void ServerUse(INetworkedLogger logger)
    {
        base.ServerUse(logger);
        List<Hex> fullMovePath = new();
        fullMovePath.Add(this.ActorHex);
        fullMovePath.AddRange(this.movePath);
        foreach(Hex hexWalkedOn in fullMovePath)
        {
            if (hexWalkedOn == this.InterruptedAtHex || hexWalkedOn == this.TargetHex)
                return;
            if (hexWalkedOn.IsEmpty())
            {
                Map.Singleton.obstacleManager.SpawnObstacleOnMap(Map.Singleton.hexGrid, hexWalkedOn.coordinates.OffsetCoordinatesAsVector(), SPAWNED_OBSTACLE_TYPE);
            }
        }
    }
}
