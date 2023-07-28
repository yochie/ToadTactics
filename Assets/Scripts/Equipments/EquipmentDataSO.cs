using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Equipments/EquipmentData", order = 0)]
public class EquipmentDataSO : ScriptableObject
{
    [SerializeField]
    private List<EquipmentSO> equipmentsList;

    //singleton loaded from resources
    private const string resourcePath = "EquipmentData";
    private static EquipmentDataSO singleton = null;
    public static EquipmentDataSO Singleton
    {
        get
        {
            if (EquipmentDataSO.singleton == null)
                EquipmentDataSO.singleton = Resources.Load<EquipmentDataSO>(resourcePath);
            return EquipmentDataSO.singleton;
        }
    }

    public List<string> GetEquipmentIDs()
    {        
        return this.equipmentsList.Select(equipment => equipment.EquipmentID).ToList();
    }

    public string GetRandomEquipmentID()
    {
        List<string> equipmentIDs = EquipmentDataSO.Singleton.GetEquipmentIDs();
        int numsIDs = equipmentIDs.Count;

        return equipmentIDs[UnityEngine.Random.Range(0, numsIDs)];
    }

    public EquipmentSO GetEquipmentByID(string equipmentIDToFind)
    {
        return this.equipmentsList.Single<EquipmentSO>(equipment => equipment.EquipmentID == equipmentIDToFind);
    }
}
