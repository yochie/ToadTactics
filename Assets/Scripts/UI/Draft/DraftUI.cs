using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DraftUI : MonoBehaviour
{
    [SerializeField]
    private List<DraftableCharacterSlotUI> draftableSlots;

    [Server]
    public void Init()
    {
        List<int> alreadyRolledIDs = new();
        foreach (DraftableCharacterSlotUI slot in draftableSlots)
        {
            int newClassID;
            do { newClassID = ClassDataSO.Singleton.GetRandomClassID(); } while (alreadyRolledIDs.Contains(newClassID));
            alreadyRolledIDs.Add(newClassID);
            slot.RpcInit(newClassID);
            slot.RpcSetButtonActiveState(true);
        }
    }

    private DraftableCharacterSlotUI GetSlotForID(int classID)
    {
        foreach (DraftableCharacterSlotUI slot in draftableSlots)
        {
            if (slot.holdsClassID == classID)
                return slot;
        }

        throw new System.Exception("No slot with given ID was found");
    }


}
