using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private int _targetFPS = 60;
    [SerializeField] RoundHandler roundHandler;
    [SerializeField] GameObject roundStartButton;
    public GameObject[] playerObjects {get; private set;}
    public Dictionary<ulong, GameObject> playerObjDict {get; private set;}
    public Dictionary<ulong, int> playerScore = new Dictionary<ulong, int>(); // TODO: make this live on server, and clients either synchronize values, or only request values

    public static Action<ulong> onPlayerObjectsUpdate;
    public static Action<ulong> onPlayerDeath;
    public static Action<ulong> onPlayerRevive;

    // Use these to let other clients know of connection/disconnection
    public static Action<ulong> onManualClientConnected;
    public static Action<ulong> onManualClientDisconnected;

    public static GameManager Instance { get; private set; }

    Color[] colorList = {Color.blue, Color.red, Color.green, Color.yellow, Color.black, Color.white};

    void Awake(){
        if (Instance != null && Instance != this){ 
            Destroy(this); 
        } 
        else{ 
            Instance = this; 
        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = _targetFPS;
    }

    void OnEnable(){

        if(playerObjDict != null) playerObjDict.Clear();
        playerObjDict = new Dictionary<ulong, GameObject>();
        
        RoundHandler.onRoundEnd += OnRoundEnd;
        // NetworkManager.Singleton.OnClientConnectedCallback += UpdatePlayerObjects;
        // NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectRpc;
        // NetworkManager.Singleton.OnClientDisconnectCallback += UpdatePlayerObjects;
        // NetworkManager.Singleton.OnClientDisconnectCallback += DisconnectPlayerObject;
        // NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectRpc;
        PlayerControllerServer.onPlayerDeath += OnPlayerDeath;
        PlayerControllerServer.onPlayerRevive += OnPlayerRevive;
    }

    void OnDisable(){
  
        playerObjDict.Clear();

        RoundHandler.onRoundEnd -= OnRoundEnd;
        // NetworkManager.Singleton.OnClientConnectedCallback -= UpdatePlayerObjects;
        // NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectRpc;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= DisconnectPlayerObject;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectRpc;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= UpdatePlayerObjects;
        PlayerControllerServer.onPlayerDeath -= OnPlayerDeath;
        PlayerControllerServer.onPlayerRevive -= OnPlayerRevive;
    }

    public void StartRound(){
        roundHandler.StartRound();
    }

    void OnRoundEnd(){
        roundStartButton.SetActive(true);
    }

    void DisconnectPlayerObject(ulong clientId){
        if(NetworkManager.Singleton.IsServer){
            playerObjDict[clientId].GetComponent<NetworkObject>().Despawn();
        }
        UpdatePlayerObjects(clientId);
    }

    void UpdatePlayerObjects(ulong clientId){
        Debug.Log("UpdatePlayerObjects()");
        playerObjects = GameObject.FindGameObjectsWithTag("Player");
        playerObjDict.Clear();
        foreach(GameObject playerObj in playerObjects){
            if(playerObj.TryGetComponent(out NetworkObject netObj)){
                playerObjDict[netObj.OwnerClientId] = playerObj;
            }
            // playerObjDict[playerObj.GetComponent<NetworkObject>().OwnerClientId] = playerObj;
        }
        Debug.Log("UpdatePlayerObjects() object count: " + playerObjects.Length);
        onPlayerObjectsUpdate?.Invoke(clientId);
    }

    /// <summary>
    /// Adds 1 score to player
    /// returns score after adding, to be used by server to send to others
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public int PlayerAddScore(ulong playerId){
        playerScore.TryGetValue(playerId, out var currScore);
        int newScore = currScore + 1;
        playerScore[playerId] = newScore;
        PlayerUIGroup.Instance.UpdateScoreUI(playerId, newScore);

        return newScore;
    }

    public void PlayerSetScore(ulong playerId, int _score){
        playerScore[playerId] = _score;
        PlayerUIGroup.Instance.UpdateScoreUI(playerId, _score);
    }

    void OnPlayerDeath(Transform player){
        onPlayerDeath?.Invoke(player.GetComponent<NetworkObject>().OwnerClientId);
    }

    void OnPlayerRevive(Transform player){
        onPlayerRevive?.Invoke(player.GetComponent<NetworkObject>().OwnerClientId);
    }

    public static Vector3 GetRandomPositionArena(){
        return new Vector3(UnityEngine.Random.Range(-6, 3), 5f, UnityEngine.Random.Range(-5, 4));
    }

    public Color GetColour(ulong clientId){
        return colorList[clientId];
    }

    public void OnClientConnect(ulong clientId){
        UpdatePlayerObjects(clientId);
    }

    public void OnClientDisconnect(ulong clientId){
        Debug.Log("Client Disconnected!");
        DisconnectPlayerObject(clientId);

    }

}
