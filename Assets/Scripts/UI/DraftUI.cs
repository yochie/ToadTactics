using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftUI : MonoBehaviour
{
    [SerializeField]
    private List<DraftableSlotUI> draftableSlots;

    public void Init()
    {
        List<int> alreadyRolledIDs = new();
        foreach (DraftableSlotUI slot in draftableSlots)
        {
            int newID;
            do { newID = ClassDataSO.Singleton.GetRandomClassID(); } while (alreadyRolledIDs.Contains(newID));
            alreadyRolledIDs.Add(newID);
            slot.RpcInit(newID);

            slot.RpcSetButtonActiveState(true);
        }        
    }

    //Called by button during draft
    public void DraftCharacter(int classID)
    {
        GameController.Singleton.LocalPlayer.CmdDraftCharacter(classID);
    }

    private DraftableSlotUI GetSlotForID(int classID)
    {
        foreach (DraftableSlotUI slot in draftableSlots)
        {
            if (slot.holdsClassID == classID)
                return slot;
        }

        throw new System.Exception("No slot with given ID was found");
    }
}
