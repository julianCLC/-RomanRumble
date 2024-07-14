using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    // TODO: Store all info of a hit event here.
    // Also Use this to store damage from different sources.
    // If damage source is player, store the info here
}

public struct DamageInfo : INetworkSerializable
{
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        
    }
}
