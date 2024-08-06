using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles items being show on player
/// Add items with PickupItem script in children
/// </summary>
public class PlayerHeldItems : NetworkBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] GameObject heldPlayerGO;

    void Awake(){
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    [Rpc(SendTo.Everyone)]
    public void ShowItemRpc(ulong objectId){
        NetworkObject netObj = NetworkManager.SpawnManager.SpawnedObjects[objectId];
        if(!netObj.gameObject.CompareTag("Player")){
            if(netObj.TryGetComponent(out ArenaItemThrowable itemScript)){
                meshFilter.mesh = itemScript.meshFilter.mesh;
                meshRenderer.material = itemScript.meshRenderer.material;
                transform.localScale = itemScript.transform.localScale;
            }

            meshRenderer.enabled = true;
        }
        else{
            // picked up player
            heldPlayerGO.SetActive(true);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void HideItemRpc(){

        meshRenderer.enabled = false;
        heldPlayerGO.SetActive(false);
    }
}
