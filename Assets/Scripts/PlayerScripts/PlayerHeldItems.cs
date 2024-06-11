using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles items being show on player
/// Add items with PickupItem script in children
/// </summary>
public class PlayerHeldItems : NetworkBehaviour
{
    // PickupItem[] itemPrefabs;
    // Dictionary<ItemType, int> itemDict = new Dictionary<ItemType, int>();
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;

    void Awake(){
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        // SetupHeldItem();
    }

    public override void OnNetworkSpawn(){
        // base.OnNetworkSpawn();
        // SetupHeldItem();
    }

    /// <summary>
    /// Populate itemPrefabs and build the dictionary
    /// </summary>
    public void SetupHeldItem(){
        /*
        int children = transform.childCount;
        itemPrefabs = new PickupItem[children];

        for(int i = 0; i < transform.childCount; i++){
            PickupItem pickupItem = transform.GetChild(i).GetComponent<PickupItem>();
            pickupItem.DisableItem();
            itemPrefabs[i] = pickupItem;
            itemDict.Add(pickupItem.itemType, i);
        }
        */
    }

    [Rpc(SendTo.Everyone)]
    // public void ShowItemRpc(MeshRenderer _meshRenderer, ItemType itemType){
    public void ShowItemRpc(ulong objectId){
        NetworkObject netObj = NetworkManager.SpawnManager.SpawnedObjects[objectId];

        if(netObj.TryGetComponent(out PickupItem itemScript)){
            meshFilter.mesh = itemScript.meshFilter.mesh;
            meshRenderer.material = itemScript.meshRenderer.material;
            transform.localScale = itemScript.transform.localScale;
        }

        meshRenderer.enabled = true;
    }

    [Rpc(SendTo.Everyone)]
    public void HideItemRpc(){

        meshRenderer.enabled = false;
    }
}
