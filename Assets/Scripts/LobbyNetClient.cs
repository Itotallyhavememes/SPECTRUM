using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using Steamworks;

public class LobbyNetClient : NetworkBehaviour
{
    void Start()
    {
        
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);

        BootstrapNetManager.instance.plrCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(SteamManager.instance.lobbyID));
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
