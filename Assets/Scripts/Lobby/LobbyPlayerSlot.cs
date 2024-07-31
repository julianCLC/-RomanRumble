using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSlot : MonoBehaviour
{

    [SerializeField] Button readyButton;
    [SerializeField] TMP_Text readyText;
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

    public void SetReadyState(bool _ready){
        if(_ready){
            readyText.text = "READY";
        }
        else{
            readyText.text = "NOT READY";
        }
    }

}
