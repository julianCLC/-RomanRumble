using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{

    [SerializeField] private GameObject[] pools;
    

    public static ObjectPoolManager instance {get; private set;}    

    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        } else { 
            instance = this; 
        } 
    }

    public T GetPoolByType<T>(PoolType poolType){

        foreach(GameObject pool in pools){
            if(pool.GetComponent<PoolInfo>()._poolType == poolType){
                return pool.GetComponent<T>();
            }
        }

        Debug.Log("POOLS | findPool() | pool '" + poolType.ToString() + "' not found"); 

        return default(T);
    }

}


// New Pool steps:
// 1) Create new script, inheriting PoolerBase
// 2) Add type to PoolType, same class name as Script
// 3) Create gameObject under ObjectPools as the spawner

// For Static FX:
// Use GenericStaticFX, and use GenericFXPooler under the gameObject

public enum PoolType{
    none,
    HitFX,
    DodgeFX,
    DeathFX
}

