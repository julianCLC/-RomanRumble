using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    [SerializeField] PlayerController pc;
    [SerializeField] PlayerControllerServer pcServer;
    [SerializeField] Animator animator;

    private string currentAnimState;

    const string IDLE = "Idle";
    const string RUN = "Run";
    const string JUMP = "JumpUp";
    const string PICKUP = "PickUp";
    const string THROW = "Throw";
    const string CHARGE = "Charging";
    const string CROUCH = "CrouchStart";
    const string CROUCHEND = "CrouchEnd";
    const string DODGE = "Dodge";
    const string HIT = "Hit";

    [SerializeField] AnimatorEvents animatorEvents;

    // Start is called before the first frame update
    void Start()
    {
        currentAnimState = IDLE;
    }

    void OnEnable(){
        animatorEvents.onPickupCall += OnPickupStart;
        animatorEvents.onThrowCall += OnThrowEnd;
    }

    void OnDisable(){
        animatorEvents.onPickupCall += OnPickupStart;
        animatorEvents.onThrowCall += OnThrowEnd;
    }

    // Update is called once per frame
    void Update()
    {
        switch (pcServer.net_currState.Value){
            // ==== MOVEMENT ====
            case MoveState.Dodge:
                ChangeAnimationState(DODGE);
                break;
            case MoveState.CrouchStart:
                ChangeAnimationState(CROUCH);
                break;

            case MoveState.CrouchEnd:
                ChangeAnimationState(CROUCHEND);
                break;

            case MoveState.Idle:
                ChangeAnimationState(IDLE);
                break;

            case MoveState.Run:
                ChangeAnimationState(RUN);
                break;

            case MoveState.Jump:
                ChangeAnimationState(JUMP);
                break;

            // ==== ACTIONS ====
            case MoveState.Pickup:
                ChangeAnimationState(PICKUP);
                break;
            case MoveState.Charging:
                ChangeAnimationState(CHARGE);
                break;
            case MoveState.Throw:
                ChangeAnimationState(THROW);
                break;

            // ==== EFFECT ====
            case MoveState.Hit:
                ChangeAnimationState(HIT);
                break;


            
        }
    }

    void OnPickupStart(){
        animator.SetLayerWeight(animator.GetLayerIndex("Hands Layer"), 1);
    }

    public void OnThrowEnd(){
        animator.SetLayerWeight(animator.GetLayerIndex("Hands Layer"), 0);
    }

    void ChangeAnimationState(string newState, int animLayer = 0){
            if(currentAnimState == newState) return;
            if(animator.GetCurrentAnimatorStateInfo(0).IsName(newState)) return; // first if statement sometimes missed
            
            animator.Play(newState, animLayer);

            currentAnimState = newState;
        }
}
