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
            slot.RpcRenderClassData(newID);

            slot.RpcSetButtonActiveState(true);
        }        
    }
}
