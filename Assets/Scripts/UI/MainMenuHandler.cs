using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuHandler : MonoBehaviour
{
    ConnectionManager connectionManagerScript;
    [SerializeField] GameObject[] allMenuGO;
    // [SerializeField] GameObject mainMenu;
    // [SerializeField] GameObject lobbyMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
