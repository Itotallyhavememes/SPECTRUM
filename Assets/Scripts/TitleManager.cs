using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject InitialButtons;
    [SerializeField] private GameObject LobbyButtons;
    private EventSystem eventSystem;

    // Start is called before the first frame update
    void Start()
    {
        eventSystem = GetComponent<EventSystem>();
        LobbyButtons?.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButton(GameObject buttonToSelect)
    {
            InitialButtons?.SetActive(false);
            LobbyButtons?.SetActive(true);
            eventSystem?.SetSelectedGameObject(buttonToSelect);
    }
}
