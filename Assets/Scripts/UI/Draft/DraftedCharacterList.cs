using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftedCharacterList : MonoBehaviour
{

    List<DraftedCharacterSlotUI> slots;

    [SerializeField]
    bool isForSelf;

    [SerializeField]
    GameObject slotPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called by unity event configured in inspector
    public void OnCharacterDrafted(int draftedByPlayerID, int draftedClassID)
    {
        bool addToThisList = false;
        
        if (this.isForSelf && draftedByPlayerID == GameController.Singleton.LocalPlayer.playerID)
            addToThisList = true;
        else if (!this.isForSelf && draftedByPlayerID != GameController.Singleton.LocalPlayer.playerID)
            addToThisList = true;

        if (!addToThisList)
            return;

        GameObject slotObject = Instantiate(this.slotPrefab, this.transform);
        DraftedCharacterSlotUI slot = slotObject.GetComponent<DraftedCharacterSlotUI>();
        slot.SetSprite(ClassDataSO.Singleton.GetSpriteByClassID(draftedClassID));
        slot.SetClassID(draftedClassID);
    }
}
