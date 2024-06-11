using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spear : PickupItem
{
    const string FloorLayerName = "Floor";
    [SerializeField] BoxCollider spearTip;
    [SerializeField] float flightTime = 1.5f;
    float flightTimer = 0;
    bool inFlight = false;

    void Update(){ 
        if(inFlight){
            if(flightTimer > 0){
                flightTimer -= Time.deltaTime;
                // transform.up = rb.velocity.normalized;
            }
            else{
                EndFlight();
            }
        }

        if(rb.velocity.normalized != Vector3.zero){
            transform.up = rb.velocity.normalized;
        }
    }

    public override void ServerThrow(ThrowInfo throwInfo)
    {
        base.ServerThrow(throwInfo);
        rb.AddForce(throwInfo.dir, ForceMode.Impulse);
        StartFlight(throwInfo.chargePercent);
    }

    protected override void OnHitPlayer(Collision collision, PlayerControllerServer pcServer, NetworkObject playerNetObj)
    {
        base.OnHitPlayer(collision, pcServer, playerNetObj);
        Debug.Log("hit player");
        if(inFlight){
            EndFlight();
        }
    }

    protected override void OnHitEnvironment(Collision collision)
    {
        base.OnHitEnvironment(collision);

        Debug.Log("hit environment");
        if(inFlight){
            EndFlight();
        }

        ContactPoint cpoint = collision.GetContact(0);
        
        if((cpoint.thisCollider == spearTip) && (collision.gameObject.layer == LayerMask.NameToLayer(FloorLayerName))){
            SpearStick();
        }
        
    }

    void StartFlight(float timerPercent){
        rb.useGravity = false;
        inFlight = true;
        flightTimer = flightTime * timerPercent; // flytime based on charge
        Debug.Log("flight time: " + flightTime + " | percent: " + timerPercent + " | current timer: " + flightTimer);
    }

    void EndFlight(){
        Debug.Log("end flight");
        flightTimer = 0;
        inFlight = false;
        rb.useGravity = true;
    }

    void SpearStick(){
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        Vector3 offset = rb.transform.up.normalized * 0.2f;
        rb.transform.position += offset;
    }
}
