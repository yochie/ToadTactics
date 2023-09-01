using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Movement", menuName = "Equipments/MovementEquipment")]
public class MovementEquipmentSO : EquipmentSO, IEquipmentQuality, IMovementModifier
{
    [field: SerializeField]
    public int MovementOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentMoveSpeed = playerCharacter.CurrentStats.moveSpeed;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, moveSpeed: currentMoveSpeed + this.MovementOffset));
    }

    public Dictionary<string, string> GetStatModificationsDictionnary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Moves", System.String.Format("+{0}", this.MovementOffset));

        return toPrint;
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int currentMoveSpeed = playerCharacter.CurrentStats.moveSpeed;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, moveSpeed: currentMoveSpeed - this.MovementOffset));
    }
}
