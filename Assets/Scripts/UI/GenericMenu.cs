using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GenericMenu : MonoBehaviour
{
    Button[] buttons;
    GameObject _selectedButton;

    void Start(){
        Initialize();

    }

    void Initialize(){
        buttons = GetComponentsInChildren<Button>();
        _selectedButton = buttons[0].gameObject;
    }

    public void OnMenuEnter(){
        if(buttons == null){
            Initialize();
        }
        EventSystem.current.SetSelectedGameObject(_selectedButton);
    }
}
