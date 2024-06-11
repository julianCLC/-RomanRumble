using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIGroup : MonoBehaviour
{
    [SerializeField] GameObject uiHandlerPrefab;
    Dictionary<ulong, PlayerUIHandler> UIHandlers = new Dictionary<ulong, PlayerUIHandler>();
    bool UIActive = false;

    public static PlayerUIGroup Instance { get; private set; }

    void Awake(){
        if (Instance != null && Instance != this){ 
            Destroy(this); 
        } 
        else{ 
            Instance = this; 
        } 
    }

    void OnEnable(){
        GameManager.onPlayerObjectsUpdate += SetPlayerUIHandlers;
    }
    
    void OnDisable(){
        GameManager.onPlayerObjectsUpdate -= SetPlayerUIHandlers;
    }

    public void UpdateScoreUI(ulong playerId, int newScore){
        if(!UIActive) return;
        UIHandlers.TryGetValue(playerId, out PlayerUIHandler ui);
        if(ui != null){ui.UpdateScore(newScore);}
    }

    public void UpdateHealthUI(ulong playerId, float healthPercent){
        if(!UIActive) return;
        UIHandlers.TryGetValue(playerId, out PlayerUIHandler ui);
        if(ui != null){ui.UpdateHealth(healthPercent);}
    }

    void SetPlayerUIHandlers(ulong newPlayerId){

        ResetUIHandlers();

        foreach(GameObject playerObj in GameManager.Instance.playerObjects){
            Debug.Log("setting");
            GameObject newHandler = Instantiate(uiHandlerPrefab);
            newHandler.transform.SetParent(transform);
            
            PlayerUIHandler playerUIHandler = newHandler.GetComponent<PlayerUIHandler>();
            
            // PlayerControllerServer pcServer = playerObj.GetComponent<PlayerControllerServer>();

            if(playerObj.TryGetComponent(out PlayerControllerServer pcServer)){
                UIHandlers.Add(pcServer.OwnerClientId, playerUIHandler);
                playerUIHandler.InitializeUI(pcServer.OwnerClientId);
            }

            // UIHandlers.Add(pcServer.OwnerClientId, playerUIHandler);

            // playerUIHandler.InitializeUI(pcServer.OwnerClientId);
        }

        UIActive = true;
    }

    void ResetUIHandlers(){
        UIHandlers.Clear();
        foreach(Transform uiObj in transform){
            Destroy(uiObj.gameObject);
        }
    }

    public void DisableUIHandlers(){
        foreach(Transform uiObj in transform){
            Destroy(uiObj.gameObject);
        }
    }
}
