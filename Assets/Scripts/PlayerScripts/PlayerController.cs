using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    // editor values
    [SerializeField] float acceleration = 0.05f;
    [SerializeField] float maxSpeed = 0.05f;
    [SerializeField] float jumpStrength = 0.125f;
    [SerializeField] float dodgeStrength = 0.225f;
    [SerializeField] float friction = 6f;
    [SerializeField] float throwStrength = 10;
    [SerializeField] float airImpulseDecay = 3f; // influenced by friction
    [SerializeField] float groundImpulseDecay = 5f; // influenced by friction
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask pickupMask;

    // movement and forces
    public Vector3 moveDirection {private set; get;}
    public Vector3 moveImpulse {private set; get;}
    public Vector3 moveForce {private set; get;}
    const float GRAVITY = -0.70f;

    // item interaction
    NetworkThrowable _itemHeld = null;
    float _chargeTimer = 0;
    [SerializeField] float maxChargeTime = 1.5f;

    // item server functionality
    bool _serverApprovedPickup = false;

    // states
    private bool _isGrounded;
    private bool _isHolding;
    private bool _inAction;
    private bool _isCharging;
    private bool _isCrouching;
    private bool _isDead = false;

    public MoveState currState {private set; get;}
    
    // components
    //[SerializeField] CameraController cameraController;
    [SerializeField] CharacterController characterController;
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] AnimatorEvents animatorEvents;
    [SerializeField] Transform playerModel;
    [SerializeField] Transform pickupArea;
    [SerializeField] Transform objectRoot;
    [SerializeField] PlayerHeldItems heldItem;
    [SerializeField] MeshRenderer playerIndicator;
    [SerializeField] GameObject[] playerMesh;

    // input
    [SerializeField] PlayerInput playerInput;
    bool _isKeyboard = false;
    public static Action onPausePressed;

    // Mouse to world
    [SerializeField] Camera mainCam; // for cursor position
    Vector3 _moveDirectionMouse;

    float _groundRay = 0.3f; // max distance for ray ground check

    // Network
    [SerializeField] PlayerControllerServer pcserver;

    // DEBUG
    [SerializeField] bool isDebugging;
    [SerializeField] Transform debug_mouseIndicator;

    void Awake(){

    }

    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AddListeners();
        if(!IsOwner){
            ConfigureOtherPlayers();
        }
        else{
            Initialize();
            playerInput.enabled = false;
        }
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        RemoveListeners();
    }

    void ConfigureOtherPlayers(){
        // Configure your version of other players
        playerInput.enabled = false; // allows clients to move their own characters
        characterController.enabled = false;
        playerIndicator.enabled = false;

        capsuleCollider.enabled = true;

        // enabled = false;
    }

    void Initialize(){
        
        // Configure Components
        characterController.enabled = true;
        // playerInput.enabled = true;
        capsuleCollider.enabled = false;
        
        _isKeyboard = playerInput.currentControlScheme.Equals("Keyboard&Mouse"); 
        mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        _groundRay = characterController.height/2f + characterController.skinWidth - characterController.radius + 0.05f;
        transform.rotation = Quaternion.Euler(0, 135, 0); // set proper rotation reference

        Debug.Log("my client id: " + OwnerClientId);

        // cameraController = GameObject.Find("CM vcam1").GetComponent<CameraController>();
        // cameraController.Initialize(transform);
        
        playerIndicator.material.color = GameManager.Instance.GetColour(OwnerClientId);
    }

    // Add/Remove listeners on network spawn/despawn instead of OnEnable
    // To make use of IsOwner
    void AddListeners(){
        GameManager.onPlayerDeath += OnPlayerDeath;
        GameManager.onPlayerRevive += OnPlayerRevive;

        if(!IsOwner) return;
        animatorEvents.onThrowCall += OnThrowHeld;
        animatorEvents.onPickupCall += OnPickupObject;
        GameManager.onGameStart += OnGameStart;
    }

    void RemoveListeners(){
        GameManager.onPlayerDeath -= OnPlayerDeath;
        GameManager.onPlayerRevive -= OnPlayerRevive;

        if(!IsOwner) return;
        animatorEvents.onThrowCall -= OnThrowHeld;
        animatorEvents.onPickupCall -= OnPickupObject;
        GameManager.onGameStart += OnGameStart;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        ChargeTimerHandler();
        MoveStateController();

        if(isDebugging){
            Debugging();
        }

        GroundCheck();
        Movement();
        ModelRotation();
        ImpulseMovement();
    }

    void FixedUpdate(){
        if(!IsOwner) return;
        characterController.Move(moveImpulse); // Apply Forces
        characterController.Move(moveForce); // Apply Movement
    }

    #region MOVEMENT
    /// <summary>
    /// Handles basic run movement
    /// Friction is applied to movement, and max speed with this method is capped
    /// Applied to moveForce
    /// </summary>
    void Movement(){
        // Set Movement
        if((currState == MoveState.Idle || currState == MoveState.Run || currState == MoveState.Jump) && !_isDead){
            moveForce += moveDirection * GetSpeed() * Time.deltaTime;
        }

        // Add Friction
        if(_isGrounded){
            Vector3 moveFriction = -moveForce * friction;
            moveForce += moveFriction * Time.deltaTime;
        }

        // Limit Movement
        moveForce = Vector3.ClampMagnitude(moveForce, maxSpeed);
    }

    /// <summary>
    /// Handles model rotation for all states
    /// </summary>
    void ModelRotation(){
        if(currState == MoveState.Idle | currState == MoveState.Run | currState == MoveState.Jump | currState == MoveState.Charging){
            if(_isCharging){
                // use mouse for aiming when on keyboard
                if(_isKeyboard){
                    ScreenSpaceToWorldSpace();
                    playerModel.rotation = Quaternion.LookRotation(_moveDirectionMouse, Vector3.up);

                    /*
                    // don't rotate player model back and forth
                    Vector3 lookAtPoint = _moveDirectionMouse - transform.position;
                    lookAtPoint = new Vector3(lookAtPoint.x, transform.position.y, lookAtPoint.z);
                    playerModel.rotation = Quaternion.LookRotation(lookAtPoint, Vector3.up);
                    */
                    
                }
                else{
                    if(moveDirection.magnitude > 0.01f){
                        playerModel.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    }
                }
            }
            else if(moveDirection.magnitude > 0){
                playerModel.rotation = Quaternion.Lerp(playerModel.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * 10f);
            }
        }
    }

    /// <summary>
    /// Handles sudden movements such as dodge and jumps
    /// Also handles external forces applied to player
    /// Applider to moveImpulse
    /// </summary>
    void ImpulseMovement(){
        if(!_isGrounded){
            // save and apply y force

            float gravityForce = moveImpulse.y;
            gravityForce += GRAVITY * Time.deltaTime;
            

            // decay impluse 
            Vector3 newImpulse = -moveImpulse * airImpulseDecay; // less decay in air
            moveImpulse += newImpulse * Time.deltaTime;

            // replace with proper y impulse
            newImpulse = moveImpulse;
            newImpulse.y = gravityForce;
            moveImpulse = newImpulse;            
        }
        else{
            Vector3 groundFriction = -moveImpulse * groundImpulseDecay;
            moveImpulse += groundFriction * Time.deltaTime;
        }
    }

    void GroundCheck(){
        if(Physics.SphereCast(transform.position + characterController.center, characterController.radius, Vector3.down, out RaycastHit hitInfo, _groundRay, groundMask, QueryTriggerInteraction.Ignore)){
            _isGrounded = true;
            // ISSUE: when jumping and landing, there is variance to the height that the player stops at
            //        when a player lands, there could be +- 0.05 variance
            //
            // TODO:  ensure that when a player is considered grounded, the height is always consistent
            //        depending on the ground height
        }
        else{
            _isGrounded = false;
        }
    }

    float GetSpeed(){
        return _isGrounded ? acceleration : acceleration*.3f; // change move influence when in air
    }

    /// <summary>
    /// Forces made by external objects applied here
    /// </summary>
    /// <param name="forceDirection"></param>
    public void AddImpulse(Vector3 forceDirection, bool hit = false){
        moveImpulse += forceDirection;
        if(hit){
            Vector3 extractedDirection = new Vector3(forceDirection.x, 0, forceDirection.z);
            // playerModel.rotation = Quaternion.LookRotation(-forceDirection.normalized);
            playerModel.rotation = Quaternion.LookRotation(-extractedDirection.normalized);
            ChangeCurrentState(MoveState.Hit, 1f);    
        }
        
    }
    
    #endregion

    #region ACTIONS
    void MoveHandler(Vector2 controllerValue){
        moveDirection = new Vector3(controllerValue.x, 0, controllerValue.y);
        moveDirection = transform.InverseTransformDirection(moveDirection);
    }

    void JumpHandler(){
        if(_isGrounded && !_isDead){
            if(!_isCrouching && !_inAction){
                // regular vertical jump
                moveImpulse = new Vector3(moveImpulse.x, jumpStrength , moveImpulse.z);
            }
            else if(_isCrouching && currState == MoveState.CrouchStart){
                // dodge
                Vector3 dodgeDirection = moveDirection.magnitude > 0.1f ? moveDirection : playerModel.forward;
                Vector3 newImpulse = dodgeDirection * dodgeStrength;
                moveImpulse += newImpulse;

                playerModel.rotation = Quaternion.LookRotation(dodgeDirection);

                ChangeCurrentState(MoveState.Dodge, 0.417f);

                float particleSize = 0.3f;
                NetworkHelperFuncs.Instance.PlayGenericFXRpc(PoolType.DodgeFX, transform.position, Vector3.up, new Vector3(particleSize, particleSize, particleSize));
            
                NetworkHelperFuncs.Instance.PlaySoundRPC("DodgeSFX");
            }
        }
    }

    void CrouchHandler(bool crouchPressed){
        if(!_isHolding && !_isDead){
            _isCrouching = crouchPressed;
        }
    }

    void OnFireDown(){
        // Pickup Item
        if(!_isDead){
            if(!_isHolding && (currState == MoveState.Idle || currState == MoveState.Run)){
                // check if any items in range
                Collider[] hitColliders = Physics.OverlapBox(pickupArea.position, pickupArea.localScale/2f, Quaternion.identity, pickupMask, QueryTriggerInteraction.Ignore);
                if(hitColliders.Length > 0){
                    float minDist = Mathf.Infinity;
                    Collider closestCollider = null;

                    // get closest item            
                    foreach(Collider collider in hitColliders){
                        if(collider.transform == transform) continue;
                        float dist = Vector3.Distance(collider.transform.position, transform.position);
                        if(dist < minDist){
                            minDist = dist;
                            closestCollider = collider;
                        }
                    }
                    
                    // start pickup
                    if(closestCollider != null){
                        _itemHeld = closestCollider.transform.GetComponent<NetworkThrowable>();

                        // play pickup animation
                        ChangeCurrentState(MoveState.Pickup, 0.333f);
                    }
                }
            }
            // Start Charging Throw
            else if(_isHolding){
                _chargeTimer = 0;
                ChangeCurrentState(MoveState.Charging);
                _isCharging = true;
            }
        }
    }

    void OnFireUp(){
        // Release Throw Charge
        if(!_isDead){
            if(_isCharging){
                _isHolding = false;
                _isCharging = false;
                
                // Play animation
                ChangeCurrentState(MoveState.Throw, 0.333f);
            }
        }
    }

    #endregion

    #region INPUT
    void OnMove(InputValue value){
        Vector2 moveInput = value.Get<Vector2>();
        MoveHandler(moveInput);
    }

    void OnJump(){
        JumpHandler();
    }

    void OnCrouch(InputValue value){
        float result = value.Get<float>();
        CrouchHandler(result > 0 ? true : false);
    }

    /// <summary>
    /// Pickup / Throw Handler
    /// </summary>
    /// <param name="value"></param>
    void OnFire(InputValue value){
        float result = value.Get<float>();
        if(currState != MoveState.Pickup){
            if(result > 0){
                OnFireDown();
            }
            else{
                OnFireUp();
            }
        }
    }

    // TODO: Take all inputs out of controller script,
    // so it can be used by anything in the game.
    // Potentially use a script with listeners that any
    // script can subscribe to
    void OnPause(InputValue value){
        PauseHandler();
        Time.timeScale = 0;
    }
    
    void OnControlsChanged(){
        if(!IsOwner) return;
        _isKeyboard = playerInput.currentControlScheme.Equals("Keyboard&Mouse");    
    }

    #endregion

    #region ANIMATION CALLS
    /// <summary>
    /// These functions are called by an event in the animation
    /// for a seamless look
    /// </summary>
    public void OnPickupObject(){
        pcserver.ItemPickupRequestRpc(new PickupInfo{
                    clientId = NetworkManager.Singleton.LocalClientId,
                    objId = _itemHeld.NetworkObjectId
                });
    }

    public void OnThrowHeld(){
        if(_itemHeld != null){
            
            float chargePercent = _chargeTimer / maxChargeTime;
            float chargePower = (chargePercent / 2) + 0.5f; // changes range from (0, 1) to (0.5, 1)

            pcserver.ItemThrowServerRpc(
                new ThrowInfo{
                    clientId = NetworkManager.Singleton.LocalClientId,
                    origin = objectRoot.position,
                    dir = playerModel.forward * throwStrength * chargePower,
                    rot = objectRoot.rotation,
                    chargePercent = chargePercent,
                    objId = _itemHeld.NetworkObjectId
                });
            
            
            heldItem.HideItemRpc();
            NetworkHelperFuncs.Instance.PlaySoundRPC("ThrowSFX");

            _itemHeld = null;
        }
    }

    // Server calls this when accepting the pickup call
    public void OnAllowPickup(){
        _isHolding = true;
        heldItem.ShowItemRpc(_itemHeld.NetworkObjectId);
        NetworkHelperFuncs.Instance.PlaySoundRPC("PickupSFX");
    }

    // Server calls this when rejecting the pickup call
    public void OnRejectPickup(){
        ReversePickup();
    }
    
    public void ReversePickup(){
        Debug.Log("reverse pickup called");
        heldItem.HideItemRpc();
        _isHolding = false;
        _itemHeld = null;
        StartCoroutine(PickupReverseDelay());
    }

    IEnumerator PickupReverseDelay(){
        yield return new WaitForSeconds(0.5f);
        animatorEvents.ManualThrowCall();
        pcserver.ResetHandsRpc();
    }
    
    #endregion

    #region ANIMATION and TIMERS
    void ChargeTimerHandler(){
        if(_isCharging && _chargeTimer < maxChargeTime){
            _chargeTimer += Time.deltaTime;
            _chargeTimer = Mathf.Min(_chargeTimer, maxChargeTime);
        }
    }
    
    void MoveStateController(){
        // Handles states when player is not doing an action
        if(!_inAction && !_isCharging){
            if(_isGrounded){
                if(_isCrouching && currState == MoveState.Dodge){
                    // if statement is here to hold the dodge pose
                    // while player is still holding crouch
                }
                else if(_isCrouching && currState != MoveState.Dodge){
                    ChangeCurrentState(MoveState.CrouchStart);
                }
                else if(!_isCrouching && (currState == MoveState.CrouchStart || currState == MoveState.Dodge)){
                    ChangeCurrentState(MoveState.CrouchEnd, 0.067f);
                }
                else if(moveDirection.magnitude > 0.1){
                    ChangeCurrentState(MoveState.Run);
                }
                else{
                    ChangeCurrentState(MoveState.Idle);
                }
            }
            else{
                ChangeCurrentState(MoveState.Jump);
            }
        }
    }

    void ChangeCurrentState(MoveState newState, float _actionTime = 0){
        if(currState == newState) return;

        currState = newState;
        
        // force play uninterrupted action
        if(_actionTime > 0){
            StartCoroutine(ActionStateTimer(_actionTime));
        }

        // Change state for others
        if(!IsOwner) return;
        pcserver.net_currState.Value = newState;
    }

    IEnumerator ActionStateTimer(float actionTime){
        _inAction = true;
        yield return new WaitForSeconds(actionTime);
        _inAction = false;
    } 

    #endregion

    #region Server confirmation
    /// <summary>
    /// Server calls this function after confirming
    /// that the item can be picked up
    /// </summary>
    public void ApprovedPickup(ulong itemPickupID){
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemPickupID, out var itemToPickup);
        if(itemToPickup != null){
            _itemHeld = itemToPickup.GetComponent<ArenaItemThrowable>();
            _isHolding = true;
            animatorEvents.ManualPickupCall();
            heldItem.ShowItemRpc(itemPickupID);
            NetworkHelperFuncs.Instance.PlaySoundRPC("PickupSFX");
        }
    }

    #endregion

    #region OTHERS

    void OnGameStart(){
        if(!IsOwner) return;
        playerInput.enabled = true;
    }

    void OnPlayerDeath(ulong clientId){  
        if(clientId == OwnerClientId){
            if(clientId == NetworkManager.Singleton.LocalClientId){
                if(_itemHeld != null || _isHolding){
                    
                    _chargeTimer = 0;
                    OnThrowHeld();
                    ResetValues();
                }

                if(_isCharging){ _isCharging = false; }

                _isDead = true;
                StartCoroutine(DeathSequence());
            }
            else{
                StartCoroutine(OtherPlayerDeathSequence());
            }
        }        
    }

    void ResetValues(){
        heldItem.HideItemRpc();
        _isHolding = false;
        _itemHeld = null;
        pcserver.ResetHandsRpc();
    }

    IEnumerator DeathSequence(){
        yield return new WaitForSeconds(0.5f);
        playerIndicator.enabled = false;
        HideMesh();

        float hitSize = 0.3f;
        NetworkHelperFuncs.Instance.PlayGenericFXRpc(PoolType.DeathFX, transform.position, Vector3.zero, new Vector3(hitSize, hitSize, hitSize));

        moveImpulse = Vector3.zero;
        moveForce = Vector3.zero;
        moveDirection = Vector3.zero;

        if(_isCrouching){ _isCrouching = false; }
        
    }

    public IEnumerator OtherPlayerDeathSequence(){
        yield return new WaitForSeconds(0.5f);
        HideMesh();
    }

    void OnPlayerRevive(ulong clientId){
        if(clientId == OwnerClientId){

            if(clientId == NetworkManager.Singleton.LocalClientId){
                StartCoroutine(ReviveSequence(clientId));
            }

            Debug.Log("client that died: " + clientId + " | my client ID: " + NetworkManager.Singleton.LocalClientId);
        }
    }

    IEnumerator ReviveSequence(ulong clientId){
        characterController.enabled = false;
        transform.position = GameManager.GetRandomPositionArena();
        characterController.enabled = true;

        yield return new WaitForSeconds(0.1f);

        // syncronize move and showmesh for other clients
        pcserver.ShowMeshRpc();

        playerIndicator.enabled = true;

        _isDead = false;
    }

    public void HideMesh(){
        foreach(GameObject mesh in playerMesh){
            mesh.SetActive(false);
        }
        
    }

    public void ShowMesh(){
        foreach(GameObject mesh in playerMesh){
            mesh.SetActive(true);
        }
        
    }

    void ScreenSpaceToWorldSpace(){
        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = mainCam.nearClipPlane;
        
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray, out raycastHit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore)){
            Vector3 mouseWorldPos = raycastHit.point;
            _moveDirectionMouse = (mouseWorldPos - transform.position).normalized;

            if(isDebugging){debug_mouseIndicator.position = mouseWorldPos;}
        }
    }

    void Debugging(){
        if(_isCharging){
            debug_mouseIndicator.gameObject.SetActive(true);
        }
        else{
            debug_mouseIndicator.gameObject.SetActive(false);
        }
    }

    void PauseHandler(){
        onPausePressed?.Invoke();
    }

    #endregion
}

public enum MoveState{
    Idle,
    Run,
    Jump,
    Pickup,
    Charging,
    Throw,
    CrouchStart,
    CrouchEnd,
    Dodge,
    Hit
}


public struct ThrowInfo : INetworkSerializable{
    public ulong clientId;
    public Vector3 origin;
    public Vector3 dir;
    public Quaternion rot;
    public float chargePercent;
    public ulong objId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref origin);
        serializer.SerializeValue(ref dir);
        serializer.SerializeValue(ref rot);
        serializer.SerializeValue(ref chargePercent);
        serializer.SerializeValue(ref objId);
    }
}

public struct PickupInfo: INetworkSerializable{
    public ulong clientId;
    public ulong objId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref objId);
    }
}