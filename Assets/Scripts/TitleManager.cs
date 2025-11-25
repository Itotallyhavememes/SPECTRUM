using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject InitialButtons;
    [SerializeField] private GameObject LobbyButtons;
    [SerializeField] private TMP_Text LobbyTitle;

    private EventSystem eventSystem;

    public static TitleManager instance;

    private void Awake()
    {
        instance = this;
        SteamManager.instance.LobbyTitle = LobbyTitle;
    }

    void Start()
    {
        eventSystem = GetComponent<EventSystem>();
        LobbyButtons?.SetActive(false);

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
            obj = LobbyButtons.GetComponentsInChildren<Button>()[0].gameObject;
        else
            obj = buttonToSelect;

        InitialButtons?.SetActive(false);
        LobbyButtons?.SetActive(true);
        eventSystem?.SetSelectedGameObject(obj);
    }

    public void LeaveButton(GameObject buttonToSelect)
    {
        GameObject obj;

        if (buttonToSelect == null)
            obj = LobbyButtons.GetComponentsInChildren<Button>()[0].gameObject;
        else
            obj = buttonToSelect;

        LobbyButtons?.SetActive(false);
        InitialButtons?.SetActive(true);
        eventSystem?.SetSelectedGameObject(obj);
    }
}
