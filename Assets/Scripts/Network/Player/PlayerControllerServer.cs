using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Runs on Server's version of other clients
/// Clients will have this component disabled for other players
/// Server will update variables on this script, that the clients can still use for things like UI
/// </summary>
public class PlayerControllerServer : NetworkBehaviour
{
    [SerializeField] PlayerController pc;
    public NetworkVariable<MoveState> net_currState = new NetworkVariable<MoveState>(MoveState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    Health _healthScript;
    private NetworkVariable<float> net_health = new NetworkVariable<float>(0);

    public static Action<Transform> onPlayerDeath;
    public static Action<Transform> onPlayerRevive;

    // this variable only lives and gets updated on server version of the player
    public ulong lastHitPlayerId {get; private set;} // this player hit me last

    [SerializeField] AnimatorEvents animatorEvents;

    public override void OnNetworkSpawn(){
        base.OnNetworkSpawn();
        
        net_health.OnValueChanged += UpdateHealthUI;
        _healthScript = GetComponent<Health>();
        
        SetupHealth();

        if(!IsServer){
            enabled = false;
            return;
        }
        else{
            InitializeOnServer();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        net_health.OnValueChanged -= UpdateHealthUI;
        
        if(IsServer){
            _healthScript.onDeath -= OnPlayerDeathServer;
        }
    }

    void InitializeOnServer(){
        
        _healthScript.onDeath += OnPlayerDeathServer;
    }

    void SetupHealth(){
        // TODO: Figure out where to set this health and do client/server consolidation
        _healthScript.InitializeHealth(10f);
        net_health.Value = _healthScript.health;
    }

    void Update(){
        if(transform.position.y < -10f){
            // OnPlayerDeathServer();
            DealDamageServer(9999f, lastHitPlayerId);
        }
    }

    [Rpc(SendTo.Server)]
    public void ItemPickupServerRpc(ulong itemPickupID, RpcParams rpcParams = default){
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemPickupID, out var itemToPickup);
        itemToPickup.TryGetComponent(out PickupItem itemScript);

        // if item is already picked up, return
        if(itemToPickup == null || itemScript.isItemHeld.Value){
            ulong clientId = rpcParams.Receive.SenderClientId;
            ReversePickupRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
            return;
        } 

        itemScript.ServerPickup(OwnerClientId);
        itemScript.ClientDespawnRpc();
        
    }

    // Tell client their pickup failed
    [Rpc(SendTo.SpecifiedInParams)]
    public void ReversePickupRpc(RpcParams rpcParams = default){
        pc.ReversePipckup();
    }

    // Reset animation layer of specific client for all
    [Rpc(SendTo.Everyone)]
    public void ResetHandsRpc(RpcParams rpcParams = default){
        animatorEvents.EndPickupOverride();
    }

    [Rpc(SendTo.Server)]
    public void ItemDropServerRpc(ThrowInfo throwInfo){
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(throwInfo.objId, out var itemToPickup);
        if (itemToPickup.TryGetComponent(out PickupItem itemScript)){
            itemScript.ServerThrow(throwInfo);
            itemScript.ClientSpawnRpc();
        }
    }

    /// <summary>
    /// Changes value on net_health on server
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamageServer(float damage, ulong hitByClientId){
        if(_healthScript != null){
            if(!_healthScript.dead){
                _healthScript.TakeDamage(damage);
                net_health.Value = _healthScript.health;
                lastHitPlayerId = hitByClientId;
            }
        }
    }

    /// <summary>
    /// Updates health UI locally
    /// Listens to when the network changes the hp value
    /// This doesn't call any functions on the healthscript locally
    /// </summary>
    /// <param name="previous"></param>
    /// <param name="current"></param>
    void UpdateHealthUI(float previous, float current){
        PlayerUIGroup.Instance.UpdateHealthUI(OwnerClientId, current / _healthScript.maxHealth);
    }

    /// <summary>
    /// This function is called when the player dies on the server side
    /// </summary>
    void OnPlayerDeathServer(){
        int newScore = GameManager.Instance.PlayerAddScore(lastHitPlayerId);

        OnPlayerDeathClientRpc(lastHitPlayerId, newScore);
        
        StartCoroutine(PlayerDeathSequence());
    }

    IEnumerator PlayerDeathSequence(){
        yield return new WaitForSeconds(3f);
        OnPlayerReviveClientRpc();
        SetupHealth();
    }

    /// <summary>
    /// When a player dies (triggered on server side)
    /// call this function on all the clients version of this player
    /// </summary>
    [Rpc(SendTo.Everyone)]
    void OnPlayerDeathClientRpc(ulong lastPlayerHitId, int newScore){
        GameManager.Instance.PlayerSetScore(lastPlayerHitId, newScore);
        onPlayerDeath?.Invoke(transform);
    }

    /// <summary>
    /// Lets all clients know that this transform has revived
    /// </summary>
    [Rpc(SendTo.Everyone)]
    void OnPlayerReviveClientRpc(){
        onPlayerRevive?.Invoke(transform);
    }

    [Rpc(SendTo.Everyone)]
    public void ShowMeshRpc(){
        pc.ShowMesh();
    }

    /// <summary>
    /// Rpc for setting impulse to the client
    /// has to be done this way since transform is client authoritative
    /// </summary>
    /// <param name="hitImpulse"></param>
    /// <param name="hit"></param>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.SpecifiedInParams)]
    public void AddImpulseRpc(Vector3 hitImpulse, bool hit, RpcParams rpcParams){
        pc.AddImpulse(hitImpulse, hit);
    }
}
