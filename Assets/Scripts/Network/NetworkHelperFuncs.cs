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
    }

    [Rpc(SendTo.Server)]
    public void DisconnectClientRpc(ulong clientId){
        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    #endregion

    #region Lobby

    [Rpc(SendTo.Everyone)]
    public void StartGameRpc(){
        onGameStart?.Invoke();
    }

    // Let server know my ready state
    [Rpc(SendTo.Server)]
    public void LobbySendReadyStateRpc(ReadyInfo _info){
        // LobbyManager.Instance.ConfigurePlayerSlot(_info);
        LobbyManager.Instance.ServerSetPlayerReadyState(_info);
    }

    // Change ready state for all clients
    [Rpc(SendTo.Everyone)]
    public void LobbyUpdatePlayerReadyRpc(ReadyInfo _info){
        LobbyManager.Instance.ConfigurePlayerSlot(_info);
    }

    // Request to update my version of players ready states
    [Rpc(SendTo.Server)]
    public void LobbyGetCurrentReadyStatesRpc(RpcParams rpcParams = default){
        ulong clientId = rpcParams.Receive.SenderClientId;

        foreach(var kvp in LobbyManager.Instance.playerSlotReadyState){
            LobbySetCurrentReadyStateRpc(
                new ReadyInfo{
                    playerId = kvp.Key,
                    readyState = kvp.Value
                },
                RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void LobbySetCurrentReadyStateRpc(ReadyInfo _info, RpcParams rpcParams = default){
        LobbyManager.Instance.ConfigurePlayerSlot(_info);
    }

    #endregion
}
