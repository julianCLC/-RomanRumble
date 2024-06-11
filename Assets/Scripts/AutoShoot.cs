using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShoot : MonoBehaviour
{
    public Rigidbody rb;
    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(shootLoop());
    }

    IEnumerator shootLoop(){

        while(true){
            rb.AddForce(transform.forward * 10, ForceMode.Impulse);
            
            yield return new WaitForSeconds(2f);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.position = transform.position;
            rb.transform.rotation = Quaternion.identity;
        }
    }
}
