using System;
using UnityEngine;

/// <summary>
/// Script for calling events through animation calls
/// </summary>
public class AnimatorEvents : MonoBehaviour
{

    public event Action onThrowCall;
    public event Action onPickupCall;

    void OnThrowCall(){
        onThrowCall?.Invoke();
    }

    void OnPickupCall(){
        onPickupCall?.Invoke();
    }

    public void EndPickupOverride(){
        // reverse a pickup call;
        Debug.Log("endpickup override");
        onThrowCall?.Invoke();
    }
}
