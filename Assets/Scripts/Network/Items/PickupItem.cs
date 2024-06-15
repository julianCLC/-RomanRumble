using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(NetworkTransform))]
public class PickupItem : NetworkBehaviour
{
    public GameObject spawnerPrefab;
    [SerializeField] public ItemType itemType;
    public MeshRenderer meshRenderer {get; private set;}
    public MeshFilter meshFilter {get; private set;}
    Collider[] itemColliders;
    protected Rigidbody rb;
    public NetworkVariable<bool> isItemHeld = new NetworkVariable<bool>();
    private ulong heldByClientId;

    float currentDamage;
    float throwCharge;

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

        if(collision.transform.CompareTag("Player") && collision.impulse.magnitude > 1.8f){

            if(collision.transform.TryGetComponent(out PlayerControllerServer pcServer) && collision.transform.TryGetComponent(out NetworkObject playerNetObj)){
                OnHitPlayer(collision, pcServer, playerNetObj);
            }
            else{
                // hitting dummies 
                OnHitEnvironment(collision);
            }

            // hit sounds and vfx
            float hitSize = 0.5f;
            NetworkHelperFuncs.Instance.PlaySoundRPC("HitSFX");
            NetworkHelperFuncs.Instance.PlayGenericFXRpc(PoolType.HitFX, collision.contacts[0].point, Vector3.zero, new Vector3(hitSize, hitSize, hitSize));
        }
        else if(!collision.transform.CompareTag("Player")){
            NetworkHelperFuncs.Instance.PlaySoundRPC("HitObjectSFX");
            OnHitEnvironment(collision);
        }

        // reduce damage
        currentDamage = currentDamage/2f;
        
    }

    protected virtual void OnHitPlayer(Collision collision, PlayerControllerServer pcServer, NetworkObject playerNetObj){

        // which direction to push the player that got hit
        Vector3 hitImpulse = -collision.GetContact(0).normal;

        Vector3 clampedHitForce = Vector3.ClampMagnitude(hitImpulse, .2f);
        pcServer.AddImpulseRpc(clampedHitForce, true, RpcTarget.Single(playerNetObj.OwnerClientId, RpcTargetUse.Temp));

        // float damageToDeal = clampedHitForce.magnitude / 0.2f;

        // float damageToDeal = currentDamage;
        Debug.Log(itemType + " dealt damage: " + currentDamage);
        pcServer.DealDamageServer(currentDamage, heldByClientId);
        NetworkHelperFuncs.Instance.PlaySoundRPC("HitSFX");

        // Debugging: Create a transform pointing in the direction of the hitImpulse
        // ArrowGenerator.Instance.GenerateArrow(collision.GetContact(0).point, hitImpulse);
    }

    protected virtual void OnHitEnvironment(Collision collision){ }

    public void ServerPickup(ulong clientId){
        if(!IsServer) return;
        isItemHeld.Value = true;
        heldByClientId = clientId;

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    public virtual void ServerThrow(ThrowInfo throwInfo){
        if(!IsServer) return;
        isItemHeld.Value = false;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.position = throwInfo.origin;
        rb.rotation = throwInfo.rot;

        currentDamage = ItemUtils.Instance.GetItemStrength(itemType) * throwInfo.chargePercent;
    }
}
