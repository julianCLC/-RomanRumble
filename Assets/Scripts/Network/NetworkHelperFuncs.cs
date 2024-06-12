using System.Collections;
using System.Collections.Generic;
using CartoonFX;
using Unity.Netcode;
using UnityEngine;

public class NetworkHelperFuncs : NetworkBehaviour
{

    public static NetworkHelperFuncs Instance {get; private set;}

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
        
        // StartNetwork.onNetworkSubScribed += DelaySubscribe;
        StartNetwork.onStartAsHost += DelaySubscribe;
        
    }


    void DelaySubscribe(){
        Debug.Log("Delay Subscribe");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectRpc;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectRpc;

    }

    void OnDisable(){
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectRpc;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectRpc;
        
    }

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

    [Rpc(SendTo.Everyone)]
    public void OnClientConnectRpc(ulong clientId){
        Debug.Log("Client Connected!");
        GameManager.Instance.OnClientConnect(clientId);
    }

    [Rpc(SendTo.Everyone)]
    public void OnClientDisconnectRpc(ulong clientId){
        GameManager.Instance.OnClientDisconnect(clientId);
    }
}
