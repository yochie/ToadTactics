using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterDataScriptableObj : ScriptableObject
{
    public List<GameObject> characterPrefabs;
    public Sprite GetSpriteForClassID(int classID)
    {
        foreach (GameObject prefab in characterPrefabs)
        {
            if(prefab.GetComponent<PlayerCharacter>().charClass.classID == classID)
            {
                return prefab.GetComponent<Sprite>();
            }
        }

        throw new Exception("Requested sprite for undocumented classID.");
    }

    public GameObject GetPrefabForClassID(int classID)
    {
        foreach (GameObject prefab in characterPrefabs)
        {
            if (prefab.GetComponent<PlayerCharacter>().charClass.classID == classID)
            {
                return prefab;
            }
        }

        throw new Exception("Requested prefab for undocumented classID.");
    }
}
