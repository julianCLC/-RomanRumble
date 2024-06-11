using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(NetworkTransform))]
public class PickupItem : NetworkBehaviour
{
    public GameObject spawnerPrefab;
    [SerializeField] float rockDamage, bombDamage, spearDamage;
    [SerializeField] public ItemType itemType;
    public MeshRenderer meshRenderer {get; private set;}
    public MeshFilter meshFilter {get; private set;}
    Collider[] itemColliders;
    protected Rigidbody rb;
    // private bool isActive = false; // can calculate hits if active
    public NetworkVariable<bool> isItemHeld = new NetworkVariable<bool>();
    private ulong heldByClientId;

    void Awake(){
        itemColliders = GetComponents<Collider>();
        rb = GetComponent<Rigidbody>();

        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
    }

    public override void OnNetworkSpawn(){
        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.Everyone)]
    public void ClientDespawnRpc(){
        // Hide this gameobject on client
        meshRenderer.enabled = false;
        foreach(Collider collider in itemColliders){
            collider.enabled = false;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ClientSpawnRpc(){
        meshRenderer.enabled = true;
        foreach(Collider collider in itemColliders){
            collider.enabled = true;
        }
    }


    /// <summary>
    /// Collision detection only happens on server version
    /// Collision effects then get sent to the client that got hit
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision){

        if(collision.transform.CompareTag("Player") && rb.velocity.magnitude >= 0.5){
            NetworkHelperFuncs.Instance.PlaySoundRPC("HitSFX");

            if(collision.transform.TryGetComponent(out PlayerControllerServer pcServer) && collision.transform.TryGetComponent(out NetworkObject playerNetObj)){
                OnHitPlayer(collision, pcServer, playerNetObj);
            }
            else{
                // hitting dummies 
                float hitSize = 0.5f;
                NetworkHelperFuncs.Instance.PlayGenericFXRpc(PoolType.HitFX, collision.contacts[0].point, Vector3.zero, new Vector3(hitSize, hitSize, hitSize));
                OnHitEnvironment(collision);
            }
        }
        else if(!collision.transform.CompareTag("Player")){
            NetworkHelperFuncs.Instance.PlaySoundRPC("HitObjectSFX");
            OnHitEnvironment(collision);
        }
        
    }

    protected virtual void OnHitPlayer(Collision collision, PlayerControllerServer pcServer, NetworkObject playerNetObj){
        Vector3 hitImpulse = -collision.GetContact(0).normal;
            if(hitImpulse.magnitude > 0.01){
                Vector3 clampedHitForce = Vector3.ClampMagnitude(hitImpulse, .2f);
                pcServer.AddImpulseRpc(clampedHitForce, true, RpcTarget.Single(playerNetObj.OwnerClientId, RpcTargetUse.Temp));

                float damageToDeal = clampedHitForce.magnitude / 0.2f;
                pcServer.DealDamageServer(damageToDeal * GetDamage(itemType), heldByClientId);
                NetworkHelperFuncs.Instance.PlaySoundRPC("HitSFX");

                float hitSize = 0.5f;
                NetworkHelperFuncs.Instance.PlayGenericFXRpc(PoolType.HitFX, collision.contacts[0].point, Vector3.zero, new Vector3(hitSize, hitSize, hitSize));

                ArrowGenerator.Instance.GenerateArrow(collision.GetContact(0).point, hitImpulse);
            }
    }

    protected virtual void OnHitEnvironment(Collision collision){
        // Debug.Log("hit object");
    }

    public void ServerPickup(ulong clientId){
        if(!IsServer) return;
        isItemHeld.Value = true;
        heldByClientId = clientId;

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    // public virtual void ServerThrow(Vector3 position, Vector3 itemRotation, Vector3 throwDirection, float chargePercent){
    public virtual void ServerThrow(ThrowInfo throwInfo){
        if(!IsServer) return;
        isItemHeld.Value = false;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.position = throwInfo.origin;
        rb.rotation = throwInfo.rot;
    }

    float GetDamage(ItemType itemType){
        switch(itemType){
            case ItemType.Rock:
                return rockDamage;

            case ItemType.Bomb:
                return bombDamage;

            case ItemType.Spear:
                return spearDamage;
        }

        return 0;
    }
}
