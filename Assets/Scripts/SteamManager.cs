using FishNet.Managing;
using FishNet.Managing.Scened;
using Steamworks;
using System;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;

    [SerializeField] private NetworkManager netManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;
    public NetworkManager GetNetManager() => netManager;

    ulong lobbyID;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

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

        Debug.Log("Finished Lobby Creation.");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        lobbyID = callback.m_ulSteamIDLobby;

        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(lobbyID), "HostAddress"));
        fishySteamworks.StartConnection(false);
    }

    public void OpenInviteGUI()
    {
        if (lobbyID != default)
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
    }

    public void StartGame()
    {
        string[] scenesToclose = { "TitleScene" };

        BootstrapNetManager.ChangeNetworkScene("TestScene", scenesToclose);
    }

    private void OnApplicationQuit()
    {

    }
}
