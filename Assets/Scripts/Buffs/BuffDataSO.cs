using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu]
public class BuffDataSO : ScriptableObject
{
    private const string resourcePath = "BuffData";

    private static BuffDataSO singleton = null;
    public static BuffDataSO Singleton
    {
        get
        {
            if (BuffDataSO.singleton == null)
                BuffDataSO.singleton = Resources.Load<BuffDataSO>(resourcePath);
            return BuffDataSO.singleton;
        }
    }

    [SerializeField]
    private List<ScriptableObject> buffDataAssets;

    internal IBuffDataSO GetBuffData(string buffDataID)
    {
        List<IBuffDataSO> buffDataList = this.buffDataAssets.Cast<IBuffDataSO>().ToList();
        return buffDataList.Single(buffData => buffData.stringID == buffDataID);
    }

    public Sprite GetBuffIcon(string buffDataID)
    {
        List<IBuffDataSO> buffDataList = this.buffDataAssets.Cast<IBuffDataSO>().ToList();
        return buffDataList.Single(buffData => buffData.stringID == buffDataID).Icon.sprite;
    }
}
