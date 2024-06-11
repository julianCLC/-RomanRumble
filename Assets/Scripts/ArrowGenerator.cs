using System.Collections.Generic;
using UnityEngine;
 
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
        // GameObject newObj = new GameObject();
        // newObj.AddComponent<Transform>();

        // newObj.transform.position = origin;
        // newObj.transform.forward = forwardPos;
        Debug.Log("trying to generate");

        transform.position = origin;
        transform.forward = forwardPos;
    }
}