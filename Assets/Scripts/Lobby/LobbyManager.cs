using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject startGameButton;
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
        Debug.Log("isServer: " + NetworkHelperFuncs.Instance.CheckIfServer());

        if(!NetworkHelperFuncs.Instance.CheckIfServer()){
            startGameButton.SetActive(false);
        }
        else{
            startGameButton.SetActive(true);
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
        Debug.Log("LobbyManager.cs | OnLeaveSession");
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
        Debug.Log("Remove player slot call");
        GameObject playerSlotGO = playerSlotGODict[playerId];
        Destroy(playerSlotGO);
        playerSlotGODict.Remove(playerId);

        if(!NetworkManager.Singleton.IsServer) return;
        playerSlotReadyState.Remove(playerId);
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
    }

    // Set player ready state locally
    public void ConfigurePlayerSlot(ReadyInfo _info){
        if(playerSlotGODict[_info.playerId].TryGetComponent(out LobbyPlayerSlot playerSlot)){
            playerSlot.SetReadyState(_info.readyState);
        }
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

public struct ReadyInfoAll : INetworkSerializable {
    public ulong[] _keys;
    public bool[] _values;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _keys);
        serializer.SerializeValue(ref _values);
    }
}
