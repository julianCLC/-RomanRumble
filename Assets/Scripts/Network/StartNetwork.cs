using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class StartNetwork : MonoBehaviour
{

    [SerializeField] NetworkConfiguring networkConfig;
    [SerializeField] TMP_Text joinCodeText;
    [SerializeField] TMP_InputField inputField;
    // [SerializeField] GameObject NetworkButtons;
    [SerializeField] GameObject[] MainMenuButtons;

    // public static Action onNetworkSubScribed;
    public static Action onStartAsHost;

    void OnEnable(){
        NetworkConfiguring.onCreateHost += HostConfigure;
        StartCoroutine(SubscribeToNetworkManagerEvents());
    }


    IEnumerator SubscribeToNetworkManagerEvents(){
        yield return new WaitUntil(() => NetworkManager.Singleton);
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
        // onNetworkSubScribed?.Invoke();
        Debug.Log("Subscribed to NetworkManager");
    
    }

    void OnDisable(){
        NetworkConfiguring.onCreateHost -= HostConfigure;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
    }

    void HostConfigure(string joinCode){


        SetJoinCode(joinCode);
        onStartAsHost?.Invoke();
    }
    void SetJoinCode(string joinCode){
        joinCodeText.text = "Join Code: " + joinCode;
    }

    public void StartHost(){
        networkConfig.StartHost();
    }

    public void StartServer(){
    }

    public void StartClient(){
        networkConfig.StartClient(inputField.text);
    }

    public void DisconnectClient(){
        networkConfig.DisconnectClient();
        foreach(GameObject button in MainMenuButtons){
            button.SetActive(true);
        }
    }

    public void QuitGame(){
        Application.Quit();
    }

    void OnConnected(ulong clientId){
        // NetworkButtons.SetActive(false);
        foreach(GameObject button in MainMenuButtons){
            button.SetActive(false);
        }
    }
    
}
