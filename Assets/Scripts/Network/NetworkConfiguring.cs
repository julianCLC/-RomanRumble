using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System;

public class NetworkConfiguring : MonoBehaviour
{

    public static event Action<string> onCreateHost;
    public static event Action onMyClientConnected;

    // Start is called before the first frame update
    private async void Start()
    {

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            //AuthenticationService.Instance.PlayerId
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateRelay(){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Join Code");

            onCreateHost?.Invoke(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            NetworkManager.Singleton.StartHost();
        } catch (RelayServiceException e){
            Debug.Log(e);
        }

    }

    private async void JoinRelay(string joinCode){
        try{
            Debug.Log("Joining relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e){
            Debug.Log(e);
        }
    }

    public void StartHost(){
        CreateRelay();
    }

    public void StartClient(string joinCode){
        JoinRelay(joinCode);
    }

    public void DisconnectClient(){
        Debug.Log("disconnect call");
        // NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        // if(!NetworkManager.Singleton.IsServer){

        // }
        NetworkManager.Singleton.Shutdown();
    }

    [Rpc(SendTo.Server)]
    public void DisconnectClientRpc(ulong clientId){
        //  NetworkManager.Singleton.Dest
        NetworkManager.Singleton.DisconnectClient(clientId);
    }
}
