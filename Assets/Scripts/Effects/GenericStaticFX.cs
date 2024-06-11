using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericStaticFX : MonoBehaviour
{
    private ParticleSystem ps;
    private Action<GenericStaticFX> _killAction;
    private float _timeToDestroy = 0;

    void Awake(){
        ps = GetComponent<ParticleSystem>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _timeToDestroy = GetLifeTime();
    }

    // Update is called once per frame
    void Update()
    {
        _timeToDestroy -= Time.deltaTime;
        if(_timeToDestroy <= 0){ _killAction(this); }
    }

    public void OnUseSetup(Action<GenericStaticFX> killAction){
        _killAction = killAction;
        // _timeToDestroy = ps.main.startLifetime.constantMax;
        _timeToDestroy = GetLifeTime();
    }  

    public void PlayFX(Vector3 position){
        transform.position = position;

        ps.Play();
    }

    float GetLifeTime(){
        float lifeTime = ps.main.startLifetime.constantMax;
        foreach (ParticleSystem particelSystem in transform.GetComponentsInChildren<ParticleSystem>()){
            if(particelSystem.main.startLifetime.constantMax > lifeTime) { lifeTime = particelSystem.main.startLifetime.constantMax; }
        }

        return lifeTime;
    }
}
