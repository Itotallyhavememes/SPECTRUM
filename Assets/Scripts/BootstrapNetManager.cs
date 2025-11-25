using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Steamworks;
using System.Linq;
using UnityEngine;

public class BootstrapNetManager : NetworkBehaviour
{
    public static BootstrapNetManager instance;
    private string currScene;
    [SerializeField] NetworkObject playerPrefab;
    private readonly SyncVar<int> playerCount = new SyncVar<int>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));

    public int plrCount
    {
        get { return playerCount.Value; }
        set { playerCount.Value = value; }
    }


    string GetCurrentScene() => currScene;

    private void Awake()
    {
        instance = this;
        playerCount.OnChange += OnPlayerCountChanged;

        // ChangePlayerSpawnData();

        //SceneManager.OnClientPresenceChangeEnd += ClientPresenceChangeEnd;
    }

    private void Start()
    {
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

    }

    [ServerRpc(RunLocally = true)]
    private void OnPlayerCountChanged(int prev, int next, bool asServer) => ChangePlrCount(next);
    [ObserversRpc]
    private void ChangePlrCount(int next)=>SteamManager.instance.LobbyTitle.text = $"{SteamMatchmaking.GetLobbyData(new CSteamID(SteamManager.instance.lobbyID), "name")}: {next}/2";
    

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
