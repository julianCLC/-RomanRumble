using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowable : NetworkThrowable
{
    public override void Pickup(PickupInfo pickupInfo){
        base.Pickup(pickupInfo);
        Debug.Log("PlayerThrowable.cs | Pickup()");
        // return ItemType.None;
    }

    public override void Throw(ThrowInfo throwInfo){
        base.Throw(throwInfo);
        Debug.Log("PlayerThrowable.cs | Throw()");
    }
}
