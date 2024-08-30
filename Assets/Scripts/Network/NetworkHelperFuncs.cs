using System;
using System.Collections;
using System.Collections.Generic;
using CartoonFX;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Serves as the script that lets other clients know if clients connect/disconnect
/// Also used for misc network functions such as soundfx or vfx
/// </summary>
public class NetworkHelperFuncs : NetworkBehaviour
{

    public static NetworkHelperFuncs Instance {get; private set;}

    // this should only be used by game manager
    // to ensure game manager can update things 
    public static Action<ulong> onJoin;  // I join session
    public static Action<ulong> onLeave; // I leave session
    public static Action<ulong> onClientJoin;
    public static Action<ulong> onClientLeave;
    public static Action onServerDisconnectClient; // server calls disconnect on client

    bool subscribed = false;

    public static Action onGameStart; // this should probably be in a different script

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(this);
            DontDestroyOnLoad(gameObject);
        }
        else {
            Instance = this;
        }
    }

    void Start(){
        ConnectionManager.onStartAsHost += DelaySubscribe;
        ConnectionManager.onDisconnect += LeaveSession;
    }
    
    void DelaySubscribe(){
        // As the host, you let other clients know of connection/disconnection of clients
        Debug.Log("Delay Subscribe");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectRpc;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectRpc;
        subscribed = true;
    }

    void OnDisable(){
        if(subscribed){
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectRpc;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectRpc;
            subscribed = false;
        }
    }

    #region Gameplay

    [Rpc(SendTo.Everyone)]
    public void PlayGenericFXRpc(PoolType poolType, Vector3 position, Vector3 forwardDir, Vector3 particleScale = new Vector3()){
        
        var pooler = ObjectPoolManager.instance.GetPoolByType<GenericFXPooler>(poolType);
        var fx = pooler.Get();
        if(forwardDir != Vector3.zero){
            fx.transform.forward = forwardDir;
        }

        if(particleScale != Vector3.zero){
            fx.transform.localScale = particleScale;
        }

        fx.PlayFX(position);
    }

    [Rpc(SendTo.Everyone)]
    public void PlaySoundRPC(string soundName){
        SoundManager.Instance.PlaySound(soundName);
    }

    #endregion

    #region Connection

    [Rpc(SendTo.Everyone)]
    public void OnClientConnectRpc(ulong clientId){
        Debug.Log("ConnectionManager.cs | Client " + clientId + " Connected!");
        
        if(clientId == NetworkManager.Singleton.LocalClientId){
            onJoin?.Invoke(clientId);
            Debug.Log("ConnectionManager.cs | I connected! (Client " + clientId + ")");
        } else{
            onClientJoin?.Invoke(clientId);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void OnClientDisconnectRpc(ulong clientId){
        Debug.Log("ConnectionManager.cs | Client " + clientId + " Disconnected!");
        onClientLeave?.Invoke(clientId);
    }

    // Local Client Left
    void LeaveSession(){
        onLeave?.Invoke(NetworkManager.Singleton.LocalClientId);

        if(subscribed){
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectRpc;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectRpc;
            subscribed = false;
        }
    }

    // client calls this to get them disconnected
    [Rpc(SendTo.Server)]
    public void DisconnectClientRpc(ulong clientId){
        if(IsServer && NetworkManager.Singleton.LocalClientId == clientId){
            // If host closes the server
            // TODO: host migration or timeout for other clients if the host disconnects

            // Disconnect all other clients
            ServerDisconnectClientRpc();

            // Shut down server
            NetworkManager.Singleton.Shutdown();
        }
        else{
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    [Rpc(SendTo.NotServer)]
    public void ServerDisconnectClientRpc(){
        onServerDisconnectClient?.Invoke();
    }

    #endregion

    #region Lobby ready states

    [Rpc(SendTo.Everyone)]
    public void StartGameRpc(){
        onGameStart?.Invoke();
    }

    // Let server know my ready state
    [Rpc(SendTo.Server)]
    public void LobbySendReadyStateRpc(LobbySlotReadyInfo _info){

        // Save ready states
        LobbyManager.Instance.ServerSetPlayerReadyState(_info);

        // Update other clients
        LobbyUpdatePlayerReadyRpc(_info);
    }

    // Change ready state for all clients
    [Rpc(SendTo.Everyone)]
    public void LobbyUpdatePlayerReadyRpc(LobbySlotReadyInfo _info){
        LobbyManager.Instance.ConfigurePlayerSlot(_info);
    }

    // Request to update my version of players ready states
    [Rpc(SendTo.Server)]
    public void LobbyGetCurrentReadyStatesRpc(RpcParams rpcParams = default){
        ulong clientId = rpcParams.Receive.SenderClientId;

        foreach(var kvp in LobbyManager.Instance.playerSlotReadyState){
            LobbySetCurrentReadyStateRpc(
                new LobbySlotReadyInfo{
                    clientId = kvp.Key,
                    readyState = kvp.Value
                },
                RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void LobbySetCurrentReadyStateRpc(LobbySlotReadyInfo _info, RpcParams rpcParams = default){
        LobbyManager.Instance.ConfigurePlayerSlot(_info);
    }

    #endregion

    #region Lobby display names

    [Rpc(SendTo.Server)]
    public void LobbySendNewNameRpc(LobbySlotNameInfo _info){
        // LobbyManager.Instance.ServerSetPlayerName(_info);

        // Save names on game manager to be used by other scripts
        LobbyManager.Instance.ServerSetPlayerName(_info);

        // call the update on other clients
        LobbyUpdatePlayerNameRpc(_info);
    }

    [Rpc(SendTo.Everyone)]
    public void LobbyUpdatePlayerNameRpc(LobbySlotNameInfo _info, RpcParams rpcParams = default){
        LobbyManager.Instance.ConfigurePlayerName(_info);
    }
    
    [Rpc(SendTo.Server)]
    public void LobbyGetCurrentNamesRpc(RpcParams rpcParams = default){
        ulong clientId = rpcParams.Receive.SenderClientId;

        foreach(var kvp in LobbyManager.Instance.playerSlotName){
            LobbySetCurrentPlayerNameRpc(
                new LobbySlotNameInfo{
                    clientId = kvp.Key,
                    playerName = kvp.Value
                },
                RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void LobbySetCurrentPlayerNameRpc(LobbySlotNameInfo _info, RpcParams rpcParams = default){
        LobbyManager.Instance.ConfigurePlayerName(_info);
    }

    // TODO: simplify updating of slots and names. maybe put all info in one

    #endregion
}
