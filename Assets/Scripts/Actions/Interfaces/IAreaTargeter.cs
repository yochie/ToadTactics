public interface IAreaTargeter
{
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }

    public static string GetAreaDescription(AreaType areaType, int areaScaler)
    {
        string areaString;

        switch (areaType)
        {
            case AreaType.arc:
                areaString = string.Format("Arcing {0} tile{1} around target", areaScaler, areaScaler > 1 ? "s" : "");
                break;
            case AreaType.pierce:
                areaString = string.Format("Line from source to target");
                break;
            case AreaType.radial:
                areaString = string.Format("Circle with {0} tile radius", areaScaler);
                break;
            case AreaType.single:
                areaString = string.Format("Single target");
                break;
            default:
                areaString = "";
                break;
        }
        return areaString;
    }
}