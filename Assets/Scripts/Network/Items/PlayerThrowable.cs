using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowable : NetworkThrowable
{
    PlayerControllerServer pcServer;
    // PlayerController pc;

    public override void OnNetworkSpawn(){
        pcServer = GetComponent<PlayerControllerServer>();
        // pc = GetComponent<PlayerController>();
    }

    public override void Pickup(PickupInfo pickupInfo){
        base.Pickup(pickupInfo);

        pcServer.HideMeshRpc();

        Debug.Log("PlayerThrowable.cs | Pickup()");
        // return ItemType.None;
    }

    public override void Throw(ThrowInfo throwInfo){
        base.Throw(throwInfo);

        pcServer.ShowMeshRpc();

        Debug.Log("PlayerThrowable.cs | Throw()");
    }
}
