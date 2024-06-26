using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacterSlotListUI : MonoBehaviour
{
    protected List<BasicCharacterSlotUI> slotList = new();

    [SerializeField]
    protected BasicCharacterSlotUI slotPrefab;

    [SerializeField]
    protected GameObject slotParent;

    public void AddBasicSlotToList(int classID)
    {        
        BasicCharacterSlotUI basicSlot = Instantiate(this.slotPrefab, this.slotParent.transform);
        basicSlot.InitForClassID(classID);
        this.slotList.Add(basicSlot);
    }

    public virtual void Init(List<int> classIDs)
    {
        foreach (int classID in classIDs)
        {
            this.AddBasicSlotToList(classID);
        }
    }
}
