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

    void OnEnable(){
        NetworkConfiguring.onCreateHost += SetJoinCode;
        StartCoroutine(SubscribeToNetworkManagerEvents());
    }


    IEnumerator SubscribeToNetworkManagerEvents(){
        yield return new WaitUntil(() => NetworkManager.Singleton);
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;

        Debug.Log("Subscribed to NetworkManager");
    
    }

    void OnDisable(){
        NetworkConfiguring.onCreateHost -= SetJoinCode;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
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
