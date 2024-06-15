using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // simple counter that is used for health, but can be used for anything that needs counting

    public float health {private set; get;}
    public float maxHealth {private set; get;}
    public bool dead {private set; get;}
    public event Action onDeath;
    public event Action onRevive;

    /// <summary>
    /// Initialize health values with maxHealth = amount
    /// </summary>
    /// <param name="amount"></param>
    public void InitializeHealth(float amount){
        maxHealth = amount;
        health = amount;
        dead = false;
    }

    public float TakeDamage(float damage){
        // applies damage to health
        // returns extra damage if amount is larger than current health
        Debug.Log("healthScript() taking damage: " + damage);
        float overkill = 0;
        health -= damage;
        if(health <= 0){
            overkill = -health;
            Death();
        }

        return overkill;
    }

    public float Heal(float heal){
        // applies healing to health
        // returns extra health if current health is larger than max health
        float overheal = 0;
        health += heal;
        if(health > maxHealth){
            overheal = health - maxHealth;
            health = maxHealth;
        }
        if(dead){ Revive(heal); }

        return overheal;
    }

    public void ModfiyMaxHealth(float newMaxHealth){
        maxHealth = newMaxHealth;
        health = Mathf.Min(health, maxHealth);
        if(health <= 0){ Death(); }
    }

    void Death(){
        if(dead) return;
        health = 0;
        dead = true;
        onDeath?.Invoke();
        
    }

    void Revive(float newHealth){
        if(!dead) return;
        dead = false;
        health = Mathf.Min(newHealth, maxHealth);
        onRevive?.Invoke();
    }
}
