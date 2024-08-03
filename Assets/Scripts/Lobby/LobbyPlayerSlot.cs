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
    [Header("Name Display/Change Area")]
    [SerializeField] TMP_InputField editNameInputField;
    [SerializeField] TMP_Text displayName;
    [SerializeField] Button changeNameButton;
    [SerializeField] Sprite changeNameSprite;
    [SerializeField] Sprite confirmNameSprite;
    [SerializeField] Image buttonImage;

    
    bool mySlot = false;
    bool isReady = false;
    bool isChangingName = false;
    public string playerName {get; private set;}

    void OnEnable(){
        if(mySlot){
            readyButton.onClick.AddListener(ToggleReady);
            changeNameButton.onClick.AddListener(OnChangeNameEvent);
        }
    }

    void OnDisable(){
        if(mySlot){
            readyButton.onClick.RemoveListener(ToggleReady);
            changeNameButton.onClick.RemoveListener(OnChangeNameEvent);
        }
    }

    public void Initialize(bool IsMySlot){
        mySlot = IsMySlot;
        if(mySlot){
            readyButton.gameObject.SetActive(true);
            readyButton.onClick.AddListener(ToggleReady);
            changeNameButton.onClick.AddListener(OnChangeNameEvent);
            playerName = "Solder";
        }
        else{
            readyButton.gameObject.SetActive(false);
            changeNameButton.gameObject.SetActive(false);
            editNameInputField.gameObject.SetActive(false);
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

    void UpdatePlayerName(string _name){
        displayName.text = _name;
    }

    void OnChangeNameEvent(){
        isChangingName = !isChangingName;

        if(isChangingName){
            OnChangeName();
        }
        else{
            OnConfirmChangeName();
        }
    }

    void OnChangeName(){
        displayName.gameObject.SetActive(false);
        editNameInputField.gameObject.SetActive(true);
        buttonImage.sprite = confirmNameSprite;

        editNameInputField.text = "";
    }

    void OnConfirmChangeName(){
        displayName.gameObject.SetActive(true);
        editNameInputField.gameObject.SetActive(false);
        buttonImage.sprite = changeNameSprite;

        playerName = editNameInputField.text;
        UpdatePlayerName(playerName);

        // let others know I changed name
    }

}
