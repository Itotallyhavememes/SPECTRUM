using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Component.Spawning;
using FishNet.Managing.Server;
using Steamworks;

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

    public static void ChangeNetworkScene(string sceneName, string[] closingScenes)
    {
        instance.CloseScenes(closingScenes);

        SceneLoadData loadDat = new SceneLoadData(sceneName);
        NetworkConnection[] connections = instance.ServerManager.Clients.Values.ToArray();
        //instance.SceneManager.LoadConnectionScenes(connections, loadDat);
        instance.SceneManager.LoadGlobalScenes(loadDat);
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

    public bool CurrentSceneIsTitle()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene";
    }

    public void LoadCurrSceneAsGlobal()
    {
        string[] name = { UnityEngine.SceneManagement.SceneManager.GetActiveScene().name };
        instance.CloseScenes(name);

        SceneLoadData loadDat = new SceneLoadData(name);

        SceneManager.LoadGlobalScenes(loadDat);
    }
}
