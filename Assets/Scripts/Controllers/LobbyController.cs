using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class LobbyController : NetworkBehaviour
{
    public static LobbyController Singleton { get; private set; }

    [SerializeField]
    private TextMeshProUGUI lanHostIPLabel;

    [SerializeField]
    private TextMeshProUGUI wanHostIPLabel;

    [SerializeField]
    private TextMeshProUGUI clientIPLabel;

    [SerializeField]
    private IPManager ipManager;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button cancelButton;

    [SerializeField]
    private Button leaveButton;

    [SerializeField]
    private SceneTransitioner sceneTransitioner;

    [SyncVar]
    private string serverLanIP = "";

    [SyncVar]
    private string serverWanIP = "";


    private void Awake()
    {
        if (LobbyController.Singleton != null)
            Destroy(LobbyController.Singleton);
        else
            LobbyController.Singleton = this;
    }

    private void Start()
    {
        if (isServer)
        {
            string lanIP = this.ipManager.GetLANIPAddress();
            this.lanHostIPLabel.text = string.Format("{0} (LAN)", lanIP);
            //save for clipboarding
            this.serverLanIP = lanIP;

            ipManager.FetchWanIP((string ip) => { this.SetWanIP(ip); });
        }
        else
        {
            //setup buttons for client side
            this.startButton.gameObject.SetActive(false);
            this.cancelButton.gameObject.SetActive(false);
            this.leaveButton.gameObject.SetActive(true);
            if (this.serverLanIP != "")
                this.lanHostIPLabel.text = this.serverLanIP;
            if (this.serverWanIP != "")
                this.wanHostIPLabel.text = this.serverWanIP;
        }
    }

    [Server]
    public void LobbyFull()
    {
        this.RpcLobbyFull();
        this.startButton.interactable = true;
    }

    [ClientRpc]
    private void RpcLobbyFull()
    {
        this.clientIPLabel.text = "Connected";
    }

    private void SetWanIP(string wanIP)
    {
        this.wanHostIPLabel.text = string.Format("{0} (WAN)", wanIP);
        //save for clipboarding
        this.serverWanIP = wanIP;
    }

    public void CopyLANToClipboard()
    {
        GUIUtility.systemCopyBuffer = this.serverLanIP;
    }

    public void CopyWANToClipboard()
    {
        GUIUtility.systemCopyBuffer = this.serverWanIP;
    }

    public void OnStartGameClicked()
    {
        if(isServer)
        {
            this.sceneTransitioner.ChangeScene(() => GameController.Singleton.CmdChangeToScene("Draft"));            
        }
    }

    public void OnCancelClicked()
    {
        if (isServer)
        {
            this.sceneTransitioner.ChangeScene(() => NetworkManager.singleton.StopHost());           
        }
    }

    //leave button is only available for clients connecting to host
    public void OnLeaveClicked()
    {
        if (!isServer)
        {
            this.sceneTransitioner.ChangeScene(() => NetworkManager.singleton.StopClient());
        }
    }
}
