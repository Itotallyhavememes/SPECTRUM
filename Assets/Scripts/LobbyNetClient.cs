using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using Steamworks;
using HeathenEngineering.SteamworksIntegration;
using FishNet.Object.Synchronizing;
using System;

public class LobbyNetClient : NetworkBehaviour
{
    private readonly SyncVar<UserData> userData = new SyncVar<UserData>();

    public UserData userDat
    {
        get { return userData.Value; }
        set { userData.Value = value; }
    }

    private void Awake()
    {

        userData.OnChange += OnUserDataUpdated;

    }

    void Start()
    {

    }

    private void OnUserDataUpdated(UserData prev, UserData next, bool asServer)
    {
        next.LoadAvatar((args) =>
        {
            if (TitleManager.instance != null)
                TitleManager.instance.onPlayerJoined.Invoke(next);
        });
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);

        BootstrapNetManager.instance.plrCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(SteamManager.instance.lobbyID));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        userDat = UserData.Me;

        Debug.Log("OnStartClient");
    }

    public override void OnDespawnServer(NetworkConnection connection)
    {
        base.OnDespawnServer(connection);
        Debug.Log($"Player Disconnecting...");

        BootstrapNetManager.instance.plrCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(SteamManager.instance.lobbyID)) - 1;
    }


    void Update()
    {

    }

}
