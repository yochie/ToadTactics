using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class MyNetworkManager : NetworkManager
{
    private bool stoppingHost;

    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new MyNetworkManager singleton { get; private set; }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Awake()
    {
        if (MyNetworkManager.singleton != null)
        {
            Debug.Log("Destroying new netmanager as singleton is already set.");
            Destroy(this.gameObject);
            return;
        }
        base.Awake();
        Debug.Log("NetworkManager awaking.");
        singleton = this;
        this.stoppingHost = false;
    }

    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    /// TODO: move this to everywhere that server might initiate scene change so that remote client can properly have fadeout triggers
    /// currently this doesn't trigger on client whenever it is initiated by a stopHost() call since networked functionalities are shutdown before callback is reached
    public override void ServerChangeScene(string newSceneName)
    {
        Debug.LogFormat("Server changing scene to {0}", newSceneName);
        if (newSceneName == SceneManager.GetActiveScene().name)
            return;

        //Dont fade out when moving to lobby scene or menu scene (menu scene fadeout always be handled via StopHost wrapper below)
        //Apparently scene name formatting for online scene is not same as that returned by SceneManager.GetActiveScene()...
        if (newSceneName == "Assets/Scenes/Lobby.unity")
        {
            base.ServerChangeScene(newSceneName);
            return;
        }

        //Finding by tag since we have a different one for each scene so it would be more trouble to get ref each time new scene is loaded
        GameObject netTransitioner = GameObject.FindWithTag("NetworkedSceneTransitioner");
        GameObject transitioner = GameObject.FindWithTag("SceneTransitioner");
        if (netTransitioner != null && GameController.Singleton != null)
        {
            Debug.Log("Found net transitioner");
            NetworkedSceneTransitioner netTransitionerComponent = netTransitioner.GetComponent<NetworkedSceneTransitioner>();
            if (!netTransitionerComponent.HasTriggered)
            {
                netTransitionerComponent.RpcFadeout();
                StartCoroutine(this.WaitForFadeouts(NetworkManager.singleton.numPlayers, () => base.ServerChangeScene(newSceneName)));
            } else
                base.ServerChangeScene(newSceneName);
        }
        else if (transitioner != null)
        {
            Debug.Log("Found local transitioner");

            SceneTransitioner transitionerComponent = transitioner.GetComponent<SceneTransitioner>();
            //make sure fadeout hasnt already occured elsewhere (e.g. in networked transitioner)
            if (!transitionerComponent.FadeOutTriggered)
                transitionerComponent.FadeOut(() => base.ServerChangeScene(newSceneName));
            else
                base.ServerChangeScene(newSceneName);
        }
        else
            base.ServerChangeScene(newSceneName);
    }

    [Server]
    public void StopHostWithTransitionsOnAllClients()
    {
        Debug.LogFormat("Stopping host with scene transitions");
        if (SceneManager.GetActiveScene().name == "Menu")
            return;

        GameObject netTransitioner = GameObject.FindWithTag("NetworkedSceneTransitioner");
        GameObject transitioner = GameObject.FindWithTag("SceneTransitioner");

        if (netTransitioner != null && GameController.Singleton != null)
        {
            netTransitioner.GetComponent<NetworkedSceneTransitioner>().RpcFadeout();
            StartCoroutine(this.WaitForFadeouts(NetworkManager.singleton.numPlayers, () => this.StopHost()));
        }
        else
        {
            //fallback on default
            Debug.Log("Couldn't find net transitioner to handle host stopping transitions across all clients. Falling back on default StopHost().");
            this.StopHost();
        }
    }

    private IEnumerator WaitForFadeouts(int numClients, Action after)
    {
        if (GameController.Singleton == null)
            yield break; ;
        
        while (GameController.Singleton.SceneFadeOutOnClients < numClients)
            yield return null;

        GameController.Singleton.SceneFadeOutOnClients = 0;
        after();
    }


    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName) { }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        //Debug.Log(GameController.Singleton);
        //Debug.Log(SceneManager.GetActiveScene().name);
        //if (GameController.Singleton == null)
        //    return;
        ////let server know that remote client is ready
        //if (!GameController.Singleton.isServer)
        //{
        //    GameController.Singleton.CmdRemoteClientFinishedLoadingScene(SceneManager.GetActiveScene().name);
        //    return;
        //}
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (this.numPlayers == 2)
        {
            //GameController.Singleton.CmdChangeToScene("Draft");
            if(LobbyController.Singleton != null)
                LobbyController.Singleton.LobbyFull();
        }
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("Client disconnected, closing host");
        //Only disconnet when remote player disconnects, local player already handles this and would cause recusion here
        if (conn != null && conn.identity != null && !conn.identity.isLocalPlayer && NetworkServer.active && !this.stoppingHost)
        {
            this.StopHost();
        }
            
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// Called on server when transport raises an exception.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect() {
        ////since Mirror cant handle this apparently...
        //Debug.Log("Client disconnected. Destroying DDOL networked objects");

        //if (GameController.Singleton != null)
        //    Destroy(GameController.Singleton.gameObject);

        //if (MasterLogger.Singleton != null)
        //    Destroy(MasterLogger.Singleton.gameObject);

    }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Called on client when transport raises an exception.</summary>
    /// </summary>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnClientError(TransportError transportError, string message) {
    }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() {
        this.stoppingHost = false;
    }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost() {
        this.stoppingHost = true;
    }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() {

    }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() {
    }

    #endregion
}
