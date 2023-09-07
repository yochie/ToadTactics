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
                areaString = string.Format("{0} tile arc", areaScaler);
                break;
            case AreaType.pierce:
                areaString = string.Format("line to target");
                break;
            case AreaType.radial:
                if (areaScaler >= Utility.MAX_DISTANCE_ON_MAP / 2)
                    areaString = "whole board";
                else
                    areaString = string.Format("{0} tile radius", areaScaler);
                break;
            case AreaType.single:
                areaString = string.Format("single tile");
                break;
            case AreaType.none:
                areaString = string.Format("");
                break;
            default:
                areaString = "";
                break;
        }
        return areaString;
    }
}