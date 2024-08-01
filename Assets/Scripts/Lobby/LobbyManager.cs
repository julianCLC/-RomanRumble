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
    [SerializeField] Button startGameButton;
    [SerializeField] GameObject playerSlotParent;
    [SerializeField] GameObject playerSlotPrefab;



    Dictionary<ulong, GameObject> playerSlotGODict = new Dictionary<ulong, GameObject>();
    public Dictionary<ulong, bool> playerSlotReadyState {get; private set;}

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
        // Instatiate playerUI prefab and add to layout group (set parent)
        GameObject newHandler = Instantiate(playerSlotPrefab);
        newHandler.transform.SetParent(playerSlotParent.transform);

        // Add to dict
        playerSlotGODict.Add(playerId, newHandler);

        // Configure playerUI
        LobbyPlayerSlot playerUI = newHandler.GetComponent<LobbyPlayerSlot>();
        playerUI.Initialize(playerId == NetworkManager.Singleton.LocalClientId);

        // keep track of ready states as server
        if(!NetworkManager.Singleton.IsServer) return;

        playerSlotReadyState.Add(playerId, false);
    }

    void RemoveLobbyPlayerSLot(ulong playerId){
        GameObject playerSlotGO = playerSlotGODict[playerId];
        Destroy(playerSlotGO);
        playerSlotGODict.Remove(playerId);

        if(!NetworkManager.Singleton.IsServer) return;
        playerSlotReadyState.Remove(playerId);

        CanStartGame();
    }

    void InitializePlayerSlots(ulong playerId = 0){
        foreach( ulong id in GameManager.Instance.connectedPlayers){
            AddLobbyPlayerSlot(id);
        }

        // Get current ready states of players from server
        if(!NetworkManager.Singleton.IsServer){
            NetworkHelperFuncs.Instance.LobbyGetCurrentReadyStatesRpc();
        }
    }

    // Save player on server
    public void ServerSetPlayerReadyState(ReadyInfo _info){
        if(!NetworkManager.Singleton.IsServer) return;
        
        playerSlotReadyState[_info.playerId] = _info.readyState;

        NetworkHelperFuncs.Instance.LobbyUpdatePlayerReadyRpc(_info);

        CanStartGame();
    }

    // Set player ready state locally
    public void ConfigurePlayerSlot(ReadyInfo _info){
        if(playerSlotGODict[_info.playerId].TryGetComponent(out LobbyPlayerSlot playerSlot)){
            playerSlot.SetReadyState(_info.readyState);
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

public struct ReadyInfo : INetworkSerializable {
    public ulong playerId;
    public bool readyState;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref readyState);
    }
}