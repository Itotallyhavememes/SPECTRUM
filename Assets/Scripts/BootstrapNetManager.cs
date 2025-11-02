using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Component.Spawning;
using FishNet.Managing.Server;

public class BootstrapNetManager : NetworkBehaviour
{
    public static BootstrapNetManager instance;
    private string currScene;
    [SerializeField] NetworkObject playerPrefab;

    string GetCurrentScene() => currScene;

    private void Awake()
    {
        instance = this;

       // ChangePlayerSpawnData();

        //SceneManager.OnClientPresenceChangeEnd += ClientPresenceChangeEnd;
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        SpawnPlayers(connection);
    }

    static void SpawnPlayers(NetworkConnection connection)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameManager.instance.titleSceneName) return;

        var obj = Instantiate(instance.playerPrefab);
        instance.Spawn(obj, connection, instance.gameObject.scene);
    }

    private static void ClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        if (args.Added)
        {

        }
    }


    public static void ChangeNetworkScene(string sceneName, string[] closingScenes)
    {
        instance.CloseScenes(closingScenes);

        SceneLoadData loadDat = new SceneLoadData(sceneName);
        NetworkConnection[] connections = instance.ServerManager.Clients.Values.ToArray();
        instance.SceneManager.LoadConnectionScenes(connections, loadDat);
        //instance.ChangePlayerSpawnData();

    }

    [ServerRpc(RequireOwnership = false)]
    void CloseScenes(string[] scenes)
    {
        CloseScenesObserver(scenes);
    }

    [ObserversRpc]
    void CloseScenesObserver(string[] scenes)
    {
        foreach (var scene in scenes)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }
    }

    void ChangePlayerSpawnData()
    {
        var playerSpawner = SteamManager.instance.GetNetManager().GetComponent<PlayerSpawner>();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameManager.instance.titleSceneName)
        {
            playerSpawner.SetPlayerPrefab(GameManager.instance.lobbyPlayerPrefab);
        }
        else
            playerSpawner.SetPlayerPrefab(GameManager.instance.gamePlayerPrefab);

        //playerSpawner.Spawns = GameManager.instance.levelSpawns[UnityEngine.SceneManagement.SceneManager.GetActiveScene().name];

    }
}
