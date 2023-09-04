using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HazardDataSO : ScriptableObject
{

    private const string resourcePath = "HazardData";

    private static HazardDataSO singleton = null;
    public static HazardDataSO Singleton
    {
        get
        {
            if (HazardDataSO.singleton == null)
                HazardDataSO.singleton = Resources.Load<HazardDataSO>(resourcePath);
            return HazardDataSO.singleton;
        }
    }

    [SerializeField]
    private List<GameObject> hazardPrefabs;
    
    [SerializeField]
    private int fireHazardDamage;
    
    [SerializeField]
    private int fireHazardStandingDamage;


    [SerializeField]
    private DamageType fireHazardDamageType;

    [SerializeField]
    private int coldHazardMovementPenalty;

    [SerializeField]
    private int appleHealingAmount;

    [SerializeField]
    private DamageType appleHealingDamageType;

    public GameObject GetHazardPrefab(HazardType typeToGet)
    {
        foreach(GameObject hazardPrefab in this.hazardPrefabs)
        {
            Hazard hazardObject = hazardPrefab.GetComponent<Hazard>();
            if (hazardObject.Type == typeToGet)
            {
                return hazardPrefab;
            }
        }

        throw new System.Exception("Could not find request hazard prefab.");
    }

    public int GetHazardDamage(HazardType hazardType, bool standingDamage = false)
    {

        switch (hazardType)
        {
            case HazardType.fire:
                if (standingDamage)
                    return this.fireHazardStandingDamage;
                else
                    return this.fireHazardDamage;

            case HazardType.apple:
                return this.appleHealingAmount;

            default:
                return 0;
        }
    }

    public DamageType GetHazardDamageType(HazardType typeToGet)
    {

        switch (typeToGet)
        {
            case HazardType.fire:
                return this.fireHazardDamageType;

            case HazardType.apple:
                return this.appleHealingDamageType;

            default:
                return DamageType.none;
        }
    }

    public int GetHazardMovementPenalty(HazardType typeToGet)
    {
        switch (typeToGet)
        {
            case HazardType.cold:
                return this.coldHazardMovementPenalty;
            default:
                return 0;
        }
    }

    internal bool IsHazardTypeRemovedWhenWalkedUpon(HazardType hazardType)
    {
        if (hazardType == HazardType.apple)
            return true;
        else
            return false;
    }
}