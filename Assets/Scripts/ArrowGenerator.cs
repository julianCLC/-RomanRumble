using System.Collections.Generic;
using UnityEngine;


// Debugging tool
// Given an origin and a forward position,
// place this transform at that point facing that direction
public class ArrowGenerator : MonoBehaviour
{
    
    public static ArrowGenerator Instance {get; private set;}

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(this);
        }
        else{
            Instance = this;
        }
    }

    public void GenerateArrow(Vector3 origin, Vector3 forwardPos){
        Debug.Log("trying to generate");

        transform.position = origin;
        transform.forward = forwardPos;
    }
}