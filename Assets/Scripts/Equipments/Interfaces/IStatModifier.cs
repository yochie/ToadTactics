using System.Collections.Generic;

public interface IStatModifier
{
    public Dictionary<string, string> GetPrintableStatDictionary();

    public void ApplyStatModification(PlayerCharacter playerCharacter);

    public void RemoveStatModification(PlayerCharacter playerCharacter);
}