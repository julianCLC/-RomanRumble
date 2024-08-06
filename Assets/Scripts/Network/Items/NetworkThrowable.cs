using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public abstract class NetworkThrowable : NetworkBehaviour, Interactable
{
    protected NetworkVariable<bool> isItemHeld = new NetworkVariable<bool>(false);
    protected ulong heldByClientId;
    [SerializeField] public ItemType itemType;

    public void Interact()
    {
        // probably want to remove this, 
        // then no use to inherit Interactable
    }

    public virtual void Pickup(PickupInfo pickupInfo){
        if(!IsServer) return;
        isItemHeld.Value = true;
        heldByClientId = pickupInfo.clientId;
    }
    public virtual void Throw(ThrowInfo throwInfo){
        if(!IsServer) return;
        isItemHeld.Value = false;
    }

    public bool CanPickUp(){
        return !isItemHeld.Value;
    }
}
