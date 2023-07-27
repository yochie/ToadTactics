using System.Collections.Generic;

public interface ITargetedAction : IAction
{
    public Hex TargetHex { get; set; }

    public List<TargetType> AllowedTargetTypes { get; set; }
}
