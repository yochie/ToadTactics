public interface IAction
{
    public abstract void CmdUse();
    //users should validate action before using it
    public abstract bool Validate();
}