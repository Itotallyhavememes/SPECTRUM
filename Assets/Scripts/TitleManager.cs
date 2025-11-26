using FishNet.Connection;
using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject initialButtons;
    [SerializeField] private GameObject lobbyButtons;
    [SerializeField] private TMP_Text lobbyTitle;
    [SerializeField] private GameObject playerList;
    [SerializeField] private GameObject playerNodePrefab;
    private Dictionary<string, GameObject> playerNodes;

    [DoNotSerialize] public UnityEvent<UserData> onPlayerJoined;
    [DoNotSerialize] public UnityEvent<UserData> onPlayerLeft;

    private EventSystem eventSystem;

    public static TitleManager instance;

    private void Awake()
    {
        instance = this;
        SteamManager.instance.LobbyTitle = lobbyTitle;

        onPlayerJoined.AddListener(OnPlayerJoined);
        onPlayerLeft.AddListener(OnPlayerLeft);

        playerNodes = new Dictionary<string, GameObject>();
    }

    void Start()
    {
        eventSystem = GetComponent<EventSystem>();
        lobbyButtons?.SetActive(false);

        if (SteamManager.instance.GetLobbyID() != default)
            StartButton(null);
    }

    void Update()
    {

    }

    public void StartButton(GameObject buttonToSelect)
    {
        GameObject obj;

        if (buttonToSelect == null)
            obj = lobbyButtons.GetComponentsInChildren<Button>()[0].gameObject;
        else
            obj = buttonToSelect;

        initialButtons?.SetActive(false);
        lobbyButtons?.SetActive(true);
        eventSystem?.SetSelectedGameObject(obj);
    }

    public void LeaveButton(GameObject buttonToSelect)
    {
        GameObject obj;

        if (buttonToSelect == null)
            obj = lobbyButtons.GetComponentsInChildren<Button>()[0].gameObject;
        else
            obj = buttonToSelect;

        lobbyButtons?.SetActive(false);
        initialButtons?.SetActive(true);
        eventSystem?.SetSelectedGameObject(obj);
    }

    private void OnPlayerJoined(UserData dat)
    {
        if (playerNodes.ContainsKey(dat.Nickname)) return;

        GameObject newNode = Instantiate(playerNodePrefab);
        newNode.transform.SetParent(playerList.transform, false);

        TMP_Text name = newNode.GetComponentInChildren<TMP_Text>();
        Image avatarImg = newNode.GetComponentInChildren<Image>();

        if (name) name.text = dat.Nickname;
        if (avatarImg) avatarImg.sprite = Sprite.Create(dat.Avatar, new Rect(0f, 0f, dat.Avatar.width, dat.Avatar.height), new Vector2(0.5f, 0.5f));

        playerNodes[dat.Nickname] = newNode;
    }

    private void OnPlayerLeft(UserData dat)
    {

    }

    private void OnDestroy()
    {
        
    }
}
