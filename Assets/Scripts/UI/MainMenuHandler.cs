using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuHandler : MonoBehaviour
{
    ConnectionManager connectionManagerScript;
    [SerializeField] GameObject[] allMenuGO;
    [SerializeField] GameObject mainMenu;
    // [SerializeField] GameObject lobbyMenu;

    void OnEnable(){
        GameManager.onGameStart += OnGameStart;
        GameManager.onLeaveSession += OnLeaveGame;
    }

    void OnDisable(){
        GameManager.onGameStart -= OnGameStart;
        GameManager.onLeaveSession -= OnLeaveGame;
    }

    public void OpenMenu(GameObject menuToOpen){
        FocusOneMenu(menuToOpen);
    }

    void FocusOneMenu(GameObject menuToOpen){
        foreach(GameObject menuGO in allMenuGO){
            if(menuGO != menuToOpen){
                menuGO.SetActive(false);
            }
            else{
                menuGO.SetActive(true);
                var menuScript = menuGO.GetComponent<GenericMenu>();
                EventSystem.current.SetSelectedGameObject(null);
                menuScript.OnMenuEnter();
                
            }
        }
    }

    void OnGameStart(){
        FocusOneMenu(null);
    }

    void OnLeaveGame(){
        FocusOneMenu(mainMenu);
    }
}
