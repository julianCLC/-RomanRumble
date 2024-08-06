using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUtils : MonoBehaviour
{
    [Serializable]
    private class ItemInfo {
        public ItemType ItemType;
        public float itemStrength;
        [SerializeField] GameObject itemPrefab;
    }

    Dictionary<ItemType, ItemInfo> itemDict = new Dictionary<ItemType, ItemInfo>();
    [SerializeField] ItemInfo[] items;

    public static ItemUtils Instance { get; private set; }

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(this);
        }
        else{
            Instance = this;
        }
    }

    void Start(){
        foreach(ItemInfo itemInfo in items){
            itemDict.Add(itemInfo.ItemType, itemInfo);
        }
    }

    public float GetItemStrength(ItemType itemType){

        return itemDict[itemType].itemStrength;
    }

}

public enum ItemType{
    None,
    Rock,
    Bomb,
    Spear,
    Player
}
