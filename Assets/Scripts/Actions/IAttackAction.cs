
public interface IAttackAction : ITargetedAction
{
    public CharacterStats AttackerStats { get; set; }

    public PlayerCharacter DefenderCharacter { get; set; }

    public CharacterStats DefenderStats { get; set; }
}
