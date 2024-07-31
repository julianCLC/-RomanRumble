using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class ConnectionManager : MonoBehaviour
{

    [SerializeField] NetworkConfiguring networkConfig;
    [SerializeField] TMP_Text joinCodeText;
    [SerializeField] TMP_InputField inputField;

    public static Action onStartAsHost;
    public static Action onDisconnect;

    void OnEnable(){
        NetworkConfiguring.onCreateHost += HostConfigure;
    }

    void OnDisable(){
        NetworkConfiguring.onCreateHost -= HostConfigure;
    }

    void HostConfigure(string joinCode){


        SetJoinCode(joinCode);
        onStartAsHost?.Invoke();
        
    }
    void SetJoinCode(string joinCode){
        joinCodeText.gameObject.SetActive(true);
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
        onDisconnect?.Invoke();
        networkConfig.DisconnectClient();
    }

    public void QuitGame(){
        Application.Quit();
    }
}
