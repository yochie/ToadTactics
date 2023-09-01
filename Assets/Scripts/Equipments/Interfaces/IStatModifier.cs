using System.Collections.Generic;

public interface IStatModifier
{
    public Dictionary<string, string> GetStatModificationsDictionnary();

    public void ApplyStatModification(PlayerCharacter playerCharacter);

    public void RemoveStatModification(PlayerCharacter playerCharacter);
}