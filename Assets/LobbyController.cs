using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI lanHostIPLabel;

    [SerializeField]
    private TextMeshProUGUI wanHostIPLabel;

    [SerializeField]
    private TextMeshProUGUI opponentIPLabel;

    [SerializeField]
    private IPManager ipManager;

    private string lanIP = "";
    private string wanIP = "";

    private void Start()
    {
        //if(MyNetworkManager.singleton.is)
        string lanIP = this.ipManager.GetLANIPAddress();
        this.lanHostIPLabel.text = string.Format("{0} (LAN)", lanIP);
        //save for clipboarding
        this.lanIP = lanIP;

        ipManager.FetchWanIP((string ip) => { this.SetWanIP(ip); });
    }

    private void SetWanIP(string wanIP)
    {
        this.wanHostIPLabel.text = string.Format("{0} (WAN)", wanIP);
        //save for clipboarding
        this.wanIP = wanIP;
    }

    public void CopyLANToClipboard()
    {
        GUIUtility.systemCopyBuffer = this.lanIP;
    }

    public void CopyWANToClipboard()
    {
        GUIUtility.systemCopyBuffer = this.wanIP;
    }


    public void OnStartGameClicked()
    {
        //if(MyNetworkManager.)
    }
}
