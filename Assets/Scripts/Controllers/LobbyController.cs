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
    private Button copyWANButton;

    [SerializeField]
    private Button copyLANButton;

    [SerializeField]
    private TMP_Dropdown lanHostSelector;

    [SerializeField]
    private GameObject lanHostSelectorContainer;

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
            List<string> lanIPs = this.ipManager.GetLANIPAddresses();
            if (lanIPs.Count == 1)
            {
                this.lanHostIPLabel.text = string.Format("{0} (LAN)", lanIPs[0]);
                //save for clipboarding
                this.serverLanIP = lanIPs[0];
            } else if (lanIPs.Count > 1)
            {
                this.lanHostIPLabel.gameObject.SetActive(false);
                this.lanHostSelectorContainer.gameObject.SetActive(true);
                List<TMP_Dropdown.OptionData> options = new();
                foreach(string lanIP in lanIPs)
                {
                    options.Add(new TMP_Dropdown.OptionData(lanIP));
                }
                this.lanHostSelector.options = options;
                this.serverLanIP = lanIPs[0];
                this.lanHostSelector.value = 0;
            }

            this.SetInteractableCopyWanButton(interactable: false);
            ipManager.FetchWanIP((string ip) => { this.SetWanIP(ip); this.SetInteractableCopyWanButton(interactable: true); });
        }
        else
        {
            //setup buttons for client side
            this.startButton.gameObject.SetActive(false);
            this.cancelButton.gameObject.SetActive(false);
            this.leaveButton.gameObject.SetActive(true);
            if (this.serverLanIP != "")
                this.lanHostIPLabel.text = this.FormatIP( this.serverLanIP, type: "LAN", hideStart: false);
            if (this.serverWanIP != "")
                this.wanHostIPLabel.text = this.FormatIP(this.serverWanIP, type: "WAN", hideStart: true);

            this.copyWANButton.gameObject.SetActive(false);
            this.copyLANButton.gameObject.SetActive(false);
        }
    }

    [Server]
    public void OnLanIPSelectorValueChanged(int valueIndex)
    {
        this.serverLanIP = this.lanHostSelector.options[valueIndex].text;
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
        //only display last three chars
        this.wanHostIPLabel.text = this.FormatIP(wanIP, type: "WAN", hideStart: true);
        //save for clipboarding
        this.serverWanIP = wanIP;
    }

    private string FormatIP(string ip, string type, bool hideStart)
    {
        if (hideStart)
            return string.Format("***.***.***.{0} ({1})", ip[(ip.LastIndexOf('.') + 1)..], type);
        else
            return string.Format("{0} ({1})", ip, type);

    }

    private void SetInteractableCopyWanButton(bool interactable)
    {
        this.copyWANButton.interactable = interactable;
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
            GameController.Singleton.CmdChangeToScene("Draft");
        }
    }

    public void OnCancelClicked()
    {
        if (isServer)
        {
            MyNetworkManager.singleton.StopHostWithTransitionsOnAllClients();
        }
    }

    //leave button is only available for clients connecting to host
    public void OnLeaveClicked()
    {
        if (!isServer)
        {
            //since Network manager doesnt handle scene change here, manually play transition instead of usual hook
            GameObject transitioner = GameObject.FindWithTag("SceneTransitioner");
            if (transitioner != null)
                transitioner.GetComponent<SceneTransitioner>().FadeOut(() => NetworkManager.singleton.StopClient());
            else
                NetworkManager.singleton.StopClient();        
        }
    }
}
