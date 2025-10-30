using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public int minPlayers;
    [SerializeField] public int maxPlayers;

    public static GameManager instance;
    public PlayerData playerData;

    [Header("--- Net Utility Variables ---")]
    [SerializeField] public string titleSceneName;
    [SerializeField] public NetworkObject gamePlayerPrefab;
    [SerializeField] public NetworkObject lobbyPlayerPrefab;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        //PlayerData tempDat = SaveManager.LoadGame(0);

        //if (tempDat != null)
        //    playerData = tempDat;
        //else
        //{
        //    Debug.LogWarning("Unable to Load Save, new save will be created.");
        //    playerData = new PlayerData();
        //}
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //    Debug.Log((SaveManager.SaveGame(playerData)) ? "Closing save successful." : "Closing save failed.");


        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    int value = 15;

        //    if (playerData.inventorySystem.Get<object>("Coins") != null)
        //    {
        //        value = playerData.inventorySystem.Get<int>("Coins");
        //        value += 15;
        //    }

        //    playerData.inventorySystem.Set("Coins", value);
        //    Debug.Log("Set coins 15");
        //    Debug.Log("Coins: " + playerData.inventorySystem.Get<int>("Coins"));
        //}
    }

    private void OnApplicationQuit()
    {
        //Debug.Log((SaveManager.SaveGame(playerData)) ? "Closing save successful." : "Closing save failed.");
    }
}