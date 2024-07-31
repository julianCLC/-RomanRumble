using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : GenericMenu
{
    [SerializeField] GameObject pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController.onPausePressed += OnPausePressed;   
    }

    void OnDisable(){
        PlayerController.onPausePressed -= OnPausePressed;
    }

    void OnPausePressed(){
        pauseMenu.SetActive(!pauseMenu.activeSelf);
    }
}
