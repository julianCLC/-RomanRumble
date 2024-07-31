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

    public void ManualThrowCall(){
        onThrowCall?.Invoke();
    }

    public void ManualPickupCall(){
        onPickupCall?.Invoke();
    }
}
