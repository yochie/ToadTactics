public interface IAreaTargeter
{
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }

    public static string GetAreaDescription(IAreaTargeter areaTargeter)
    {
        string areaString;

        switch (areaTargeter.TargetedAreaType)
        {
            case AreaType.arc:
                areaString = string.Format("Area draws an arc of {0} tiles around target", areaTargeter.AreaScaler);
                break;
            case AreaType.pierce:
                areaString = string.Format("Area includes all tiles on a line from source to target");
                break;
            case AreaType.radial:
                areaString = string.Format("Area draws a circle with {0} tile radius around target", areaTargeter.AreaScaler);
                break;
            case AreaType.single:
                areaString = string.Format("Area targets a single tile");
                break;
            default:
                areaString = "undefined area";
                break;
        }
        return areaString;
    }
}