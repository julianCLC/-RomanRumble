using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSlot : MonoBehaviour
{

    [SerializeField] Button readyButton;
    [SerializeField] TMP_Text readyButtonText;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Image background;
    
    bool mySlot = false;
    bool isReady = false;

    void OnEnable(){
        if(mySlot){
            readyButton.onClick.AddListener(ToggleReady);
        }
    }

    void OnDisable(){
        if(mySlot){
            readyButton.onClick.RemoveListener(ToggleReady);
        }
    }

    public void Initialize(bool IsMySlot){
        mySlot = IsMySlot;
        if(mySlot){
            readyButton.gameObject.SetActive(true);
            readyButton.onClick.AddListener(ToggleReady);
        }
        else{
            readyButton.gameObject.SetActive(false);
        }
    }

    void ToggleReady(){
        isReady = !isReady;
        // call rpc
        NetworkHelperFuncs.Instance.LobbySendReadyStateRpc(new ReadyInfo{
            playerId = NetworkManager.Singleton.LocalClientId,
            readyState = isReady
        });
    }

    void UpdateBackground(bool _ready){
         (Sprite bgSprite, Sprite readySprite) = LobbyManager.Instance.GetReadySprite(_ready);
        background.sprite = bgSprite;
        readyButton.image.sprite = readySprite;
        
        if(_ready){
            readyButtonText.text = "UNREADY";
        }
        else{
            readyButtonText.text = "READY";
        }
    }

    public void SetReadyState(bool _ready){
        if(_ready){
            readyText.text = "READY";
        }
        else{
            readyText.text = "NOT READY";
        }

        UpdateBackground(_ready);
    }

}
