using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{

    [SerializeField] private GameObject prefab;

    private const int MaxPrefabCount = 10;

    public static ItemSpawner Singleton { get; private set; }

    void Awake(){
        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
        } else {
            Singleton = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnItemStart;
    }

    private void SpawnItemStart(){
        NetworkManager.Singleton.OnServerStarted -= SpawnItemStart;
        for(int i = 0; i < 3; i++){
            // SpawnItem();
        }
    }

    // Spawn in world
    private NetworkObject SpawnItem(){
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GameManager.GetRandomPositionArena(), Quaternion.identity);
        PickupItem pickupItem = obj.transform.GetComponent<PickupItem>();
        pickupItem.spawnerPrefab = prefab;
        obj.Spawn(true);

        return obj;
    }

    /*
    public void SpawnAndThrowItem(Vector3 throwDir, Vector3 position){
        // TODO: Generalize to any item
        NetworkObject obj = SpawnItem();
        obj.Spawn(true);
        obj.transform.position = position;
        obj.transform.GetComponent<Rigidbody>().AddForce(throwDir, ForceMode.Impulse);

    }
    */
}

