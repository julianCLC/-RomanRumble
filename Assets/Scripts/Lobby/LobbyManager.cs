using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject playerSlotParent;
    [SerializeField] GameObject playerSlotPrefab;

    Dictionary<ulong, GameObject> playerSlotDict = new Dictionary<ulong, GameObject>();
    Dictionary<ulong, bool> playerSlotReadyState = new Dictionary<ulong, bool>();

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
        InitializePlayerSlots();
    }


    void OnLeaveSession(){
        Debug.Log("LobbyManager.cs | OnLeaveSession");
        var playerSlotGOs = playerSlotDict.Values;
        foreach(GameObject playerSlotGO in playerSlotGOs){
            Destroy(playerSlotGO);
        }

        playerSlotDict.Clear();
    }

    void AddLobbyPlayerSlot(ulong playerId){
        // Instatiate playerUI prefab and add to layout group (set parent)
        GameObject newHandler = Instantiate(playerSlotPrefab);
        newHandler.transform.SetParent(playerSlotParent.transform);

        // Add to dict
        playerSlotDict.Add(playerId, newHandler);

        // Configure playerUI
        LobbyPlayerSlot playerUI = newHandler.GetComponent<LobbyPlayerSlot>();
        playerUI.Initialize(playerId == NetworkManager.Singleton.LocalClientId);
    }

    void RemoveLobbyPlayerSLot(ulong playerId){
        Debug.Log("Remove player slot call");
        GameObject playerSlotGO = playerSlotDict[playerId];
        Destroy(playerSlotGO);
        playerSlotDict.Remove(playerId);
    }

    void InitializePlayerSlots(ulong playerId = 0){
        foreach( ulong id in GameManager.Instance.connectedPlayers){
            AddLobbyPlayerSlot(id);
        }
    }

    public void ConfigurePlayerSlot(ReadyInfo _info){
        if(playerSlotDict[_info.playerId].TryGetComponent(out LobbyPlayerSlot playerSlot)){
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
