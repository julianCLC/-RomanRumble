using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject playerSlotParent;
    [SerializeField] GameObject playerSlotPrefab;
    [Header("UI Elements")]
    [SerializeField] Button startGameButton;
    [SerializeField] Sprite notReadySprite;
    [SerializeField] Sprite readySprite;
    [SerializeField] Sprite notReadyButtonSprite;
    [SerializeField] Sprite readyButtonSprite;


    Dictionary<ulong, GameObject> playerSlotGODict = new Dictionary<ulong, GameObject>();
    public Dictionary<ulong, bool> playerSlotReadyState {get; private set;}
    public Dictionary<ulong, string> playerSlotName {get; private set;} // TODO: this should be available in a centralized player info struct

    public static LobbyManager Instance {get; private set;}

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(this);
            DontDestroyOnLoad(gameObject);
        }
        else {
            Instance = this;
        }
    }

    void OnEnable(){
        GameManager.onJoinSession += OnJoinSession;
        GameManager.onLeaveSession += OnLeaveSession;
        GameManager.onManualClientConnected += AddLobbyPlayerSlot;
        GameManager.onManualClientDisconnected += RemoveLobbyPlayerSLot;
    }

    void OnDisable(){
        GameManager.onJoinSession -= OnJoinSession;
        GameManager.onLeaveSession -= OnLeaveSession;
        GameManager.onManualClientConnected -= AddLobbyPlayerSlot;
        GameManager.onManualClientDisconnected -= RemoveLobbyPlayerSLot;
    }

    void OnJoinSession(){
        if(!NetworkManager.Singleton.IsServer){
            startGameButton.gameObject.SetActive(false);
        }
        else{
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = false;

            if(playerSlotReadyState == null){
                playerSlotReadyState = new Dictionary<ulong, bool>();
            }
            else{
                playerSlotReadyState.Clear();
            }

            if(playerSlotName == null){
                playerSlotName = new Dictionary<ulong, string>();
            }
            else{
                playerSlotName.Clear();
            }
            

        }

        InitializePlayerSlots();
    }

    void OnLeaveSession(){
        var playerSlotGOs = playerSlotGODict.Values;
        foreach(GameObject playerSlotGO in playerSlotGOs){
            Destroy(playerSlotGO);
        }

        playerSlotGODict.Clear();
    }

    void AddLobbyPlayerSlot(ulong playerId){
        Debug.Log("adding slot");
        // Instatiate playerUI prefab and add to layout group (set parent)
        GameObject newHandler = Instantiate(playerSlotPrefab);
        newHandler.transform.SetParent(playerSlotParent.transform);

        // Add to dict
        playerSlotGODict.Add(playerId, newHandler);

        // Configure playerUI
        LobbyPlayerSlot newLobbySlot = newHandler.GetComponent<LobbyPlayerSlot>();
        newLobbySlot.Initialize(playerId == NetworkManager.Singleton.LocalClientId);

        // keep track of ready states as server
        if(!NetworkManager.Singleton.IsServer) return;

        playerSlotReadyState.Add(playerId, false);
        playerSlotName.Add(playerId, newLobbySlot.playerName);
    }

    void RemoveLobbyPlayerSLot(ulong playerId){
        GameObject playerSlotGO = playerSlotGODict[playerId];
        Destroy(playerSlotGO);
        playerSlotGODict.Remove(playerId);

        if(!NetworkManager.Singleton.IsServer) return;
        playerSlotReadyState.Remove(playerId);
        playerSlotName.Remove(playerId);

        CanStartGame();
    }

    void InitializePlayerSlots(ulong playerId = 0){
        if(NetworkManager.Singleton.IsServer){
            string connectedList = "";
            foreach(ulong id in GameManager.Instance.connectedPlayers){
                connectedList += id + ", ";
            }

            Debug.Log("LobbyManager.cs | InitializePlayerSlots() | game manager connected players list: " + connectedList);
        }


        foreach(ulong id in GameManager.Instance.connectedPlayers){
            AddLobbyPlayerSlot(id);
        }

        // Get current ready states of players from server
        if(!NetworkManager.Singleton.IsServer){
            NetworkHelperFuncs.Instance.LobbyGetCurrentReadyStatesRpc();
            NetworkHelperFuncs.Instance.LobbyGetCurrentNamesRpc();
        }
    }

    public (Sprite, Sprite) GetReadySprite(bool isReady){
        if(isReady){
            return (readySprite, notReadyButtonSprite);
        }
        else{
            return (notReadySprite, readyButtonSprite);
        }
    }

    // Save player on server
    public void ServerSetPlayerReadyState(LobbySlotReadyInfo _info){
        if(!NetworkManager.Singleton.IsServer) return;
        
        playerSlotReadyState[_info.clientId] = _info.readyState;

        // NetworkHelperFuncs.Instance.LobbyUpdatePlayerReadyRpc(_info);

        CanStartGame();
    }

    // Set player ready state locally
    public void ConfigurePlayerSlot(LobbySlotReadyInfo _info){
        if(playerSlotGODict[_info.clientId].TryGetComponent(out LobbyPlayerSlot playerSlot)){
            playerSlot.SetReadyState(_info.readyState);
        }
    }

    
    // Save player on server
    public void ServerSetPlayerName(LobbySlotNameInfo _info){
        if(!NetworkManager.Singleton.IsServer) return;
        
        playerSlotName[_info.clientId] = _info.playerName;

        // NetworkHelperFuncs.Instance.LobbyUpdatePlayerNameRpc(_info);
    }
    
    public void ConfigurePlayerName(LobbySlotNameInfo _info){
        if(playerSlotGODict[_info.clientId].TryGetComponent(out LobbyPlayerSlot playerSlot)){
            playerSlot.UpdatePlayerName(_info.playerName);
        }
    }

    void CanStartGame(){
        foreach(bool readyState in playerSlotReadyState.Values){
            if(!readyState){
                startGameButton.interactable = false;
                return;
            }
        }

        startGameButton.interactable = true;
    }
}

public struct LobbySlotReadyInfo : INetworkSerializable {
    public ulong clientId;
    public bool readyState;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref readyState);
    }
}

public struct LobbySlotNameInfo : INetworkSerializable {
    public ulong clientId;
    public string playerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
    }
}