using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Range", menuName = "Equipments/MovementEquipment")]
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

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Movement", System.String.Format("+{0}", this.MovementOffset));

        return toPrint;
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int currentMoveSpeed = playerCharacter.CurrentStats.moveSpeed;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, moveSpeed: currentMoveSpeed - this.MovementOffset));
    }
}
