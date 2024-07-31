using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] GameObject playerUIPrefab;
    Dictionary<ulong, PlayerUI> UIHandlers = new Dictionary<ulong, PlayerUI>();
    bool UIActive = false;

    public static PlayerUIManager Instance { get; private set; }

    void Awake(){
        if (Instance != null && Instance != this){ 
            Destroy(this); 
        } 
        else{ 
            Instance = this; 
        } 
    }

    void OnEnable(){
        // GameManager.onPlayerObjectsUpdate += SetPlayerUIHandlers;
        GameManager.onJoinSession += InitializePlayerUI;
        GameManager.onLeaveSession += ResetUIHandlers;
        GameManager.onManualClientConnected += AddPlayerUI;
        GameManager.onManualClientDisconnected += RemovePlayerUI;
    }
    
    void OnDisable(){
        // GameManager.onPlayerObjectsUpdate -= SetPlayerUIHandlers;
        GameManager.onJoinSession -= InitializePlayerUI;
        GameManager.onLeaveSession -= ResetUIHandlers;
        GameManager.onManualClientConnected -= AddPlayerUI;
        GameManager.onManualClientDisconnected -= RemovePlayerUI;
    }

    void AddPlayerUI(ulong playerId){
        Debug.Log("Adding player handler");
        // Instatiate playerUI prefab and add to layout group (set parent)
        GameObject newHandler = Instantiate(playerUIPrefab);
        newHandler.transform.SetParent(transform);

        // Configure playerUI
        PlayerUI playerUI = newHandler.GetComponent<PlayerUI>();
        playerUI.InitializeUI(playerId);

        // Add to ui handlers list
        UIHandlers.Add(playerId, playerUI);
    }

    void RemovePlayerUI(ulong playerId){
        PlayerUI playerUI = UIHandlers[playerId];

        // Destroy Gameobject
        Destroy(playerUI.gameObject);

        // Remove from list
        UIHandlers.Remove(playerId);
    }

    public void UpdateScoreUI(ulong playerId, int newScore){
        if(!UIActive) return;
        UIHandlers.TryGetValue(playerId, out PlayerUI ui);
        if(ui != null){ui.UpdateScore(newScore);}
    }

    public void UpdateHealthUI(ulong playerId, float healthPercent){
        if(!UIActive) return;
        UIHandlers.TryGetValue(playerId, out PlayerUI ui);
        if(ui != null){ui.UpdateHealth(healthPercent);}
    }

    void InitializePlayerUI(){
        
        foreach(ulong playerId in GameManager.Instance.connectedPlayers){
            AddPlayerUI(playerId);
        }

        UIActive = true;
    }

    void ResetUIHandlers(){
        // Reset dictionary
        UIHandlers.Clear();

        // Destroy all children
        foreach(Transform uiObj in transform){
            Debug.Log("deleting");
            Destroy(uiObj.gameObject);
        }
    }

    public void DisableUIHandlers(){
        foreach(Transform uiObj in transform){
            Destroy(uiObj.gameObject);
        }
    }
}
