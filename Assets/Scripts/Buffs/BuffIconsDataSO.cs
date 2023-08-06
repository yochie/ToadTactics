using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BuffIconsDataSO : ScriptableObject
{
    private const string resourcePath = "BuffIconsData";

    private static BuffIconsDataSO singleton = null;
    public static BuffIconsDataSO Singleton
    {
        get
        {
            if (BuffIconsDataSO.singleton == null)
                BuffIconsDataSO.singleton = Resources.Load<BuffIconsDataSO>(resourcePath);
            return BuffIconsDataSO.singleton;
        }
    }

    [SerializeField]
    private List<Sprite> buffIcons;

    public Sprite GetBuffIcon(string name)
    {
        foreach(Sprite icon in this.buffIcons)
        {
            Debug.Log(icon.name);
            if (icon.name == name)
            {
                return icon;
            }
        }

        throw new System.Exception("Couldn't find buff icon with requested name");
    }
}
