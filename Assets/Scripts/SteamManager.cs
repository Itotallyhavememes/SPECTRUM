using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using Steamworks;
using System;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;

    public TMP_Text LobbyTitle;
    [SerializeField] private NetworkManager netManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;
    public NetworkManager GetNetManager() => netManager;

    public ulong lobbyID { get; private set; }

    public ulong GetLobbyID() => lobbyID;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<SteamNetConnectionStatusChangedCallback_t> remoteConnectionChanged;

    private void Awake()
    {
        instance = this;

        string[] args = Environment.GetCommandLineArgs();

        if (args.Length > 0)
        {
            string id = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "+connect_lobby" && i + 1 < args.Length)
                {
                    id = args[i + 1];
                    Debug.Log($"LobbyID: {id}");
                    SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(id)));
                }
            }

        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnServerRemoteConnectionState;

    }


    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, GameManager.instance.maxPlayers);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log($"OnLobbyCreated: {callback.m_eResult.ToString()}");

        if (callback.m_eResult != EResult.k_EResultOK) return;

        lobbyID = callback.m_ulSteamIDLobby;

        SteamMatchmaking.SetLobbyData(new CSteamID(lobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(lobbyID), "name", $"{SteamFriends.GetPersonaName()}'s Lobby");

        fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        fishySteamworks.StartConnection(true);

        BootstrapNetManager.instance.LoadCurrSceneAsGlobal();

        Debug.Log("Finished Lobby Creation.");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        if (lobbyID != default) LeaveLobby();
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        lobbyID = callback.m_ulSteamIDLobby;

        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(lobbyID), "HostAddress"));

        fishySteamworks.StartConnection(false);

        bool isTitleScene = BootstrapNetManager.instance.CurrentSceneIsTitle();

        if (isTitleScene)
            TitleManager.instance?.StartButton(null);
    }
    public void OnServerRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.LogWarning($"User has either left or joined, updating lobby title: {args.ConnectionState.ToString()}");
    }

    public void OpenInviteGUI()
    {
        SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(lobbyID));
    }

    public void OpenJoinGUI()
    {
        SteamFriends.ActivateGameOverlay("Friends");
    }

    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
        lobbyID = default;

        fishySteamworks.StopConnection(false);

        if (netManager.IsServerStarted)
            fishySteamworks.StopConnection(true);

        if (LobbyTitle) LobbyTitle.text = "";
    }

    public void StartGame()
    {
        string[] scenesToclose = { "TitleScene" };

        SteamMatchmaking.SetLobbyJoinable(new CSteamID(lobbyID), false);

        BootstrapNetManager.ChangeNetworkScene("TestScene", scenesToclose);

    }

    private void OnApplicationQuit()
    {

    }
}
