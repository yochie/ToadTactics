using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class DraftableSlotUI : NetworkBehaviour
{
    [SerializeField]
    private Image spriteImage;

    [SerializeField]
    private TextMeshProUGUI nameLabel;

    [SerializeField]
    private TextMeshProUGUI descriptionLabel;



    [ClientRpc]
    public void RpcRenderClassData(int classID)
    {

    }
}
