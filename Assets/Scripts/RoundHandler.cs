using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Currently not being used
/// </summary>
public class RoundHandler : NetworkBehaviour
{
    // settings
    [SerializeField] float initRoundTime;


    NetworkVariable<bool> roundActive = new NetworkVariable<bool>(false);
    float roundTimer;

    // events
    public static event Action onRoundStart;
    public static event Action onRoundEnd;

    public void StartRound(float _roundTime = -1){
        if(!IsServer) return;
        // configure
        float roundTimeToPass;
        if(_roundTime > 0){
            roundTimeToPass = _roundTime;
        }
        else{
            roundTimeToPass = initRoundTime;
        }
        InitializeRoundRpc(roundTimeToPass);

        roundActive.Value = true;

        // onRoundStart?.Invoke();
    }

    [Rpc(SendTo.Everyone)]
    void InitializeRoundRpc(float _roundTime){
        roundTimer = _roundTime;
        onRoundStart?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if(roundActive.Value){
            roundTimer -= Time.deltaTime;
            // Debug.Log("round timer: " + roundTimer);
            if(roundTimer < 0){
                RoundEnd();
            }
        }
    }

    void RoundEnd(){
        if(!IsServer) return;
        roundActive.Value = false;

        onRoundEnd?.Invoke();
    }

}

enum RoundStates {
    Standby, // waiting for players, etc
    Start, // countdown
    Play, // round in progress
    End // round just finished
}