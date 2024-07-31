using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cmVirtualCam;
    [SerializeField] CinemachineTargetGroup cmTargetGroup;
    [SerializeField] Transform cameraOrigin;
    private Transform playerToFollow;
    private Vector3 posToMove;
    List<Transform> playerList = new List<Transform>();

    [SerializeField] float smoothTime = 1f;
    [SerializeField] float minZoom = 8f;
    [SerializeField] float maxZoom = 20f;
    float zoomLimiiter = 12f; // zoom range from 8 - 20

    private Vector3 camVelocity;

    bool isActive = false;

    void OnEnable(){
        GameManager.onPlayerDeath += RemoveWeightFromTargetGroup;
        GameManager.onPlayerRevive += AddWeightFromTargetGroup;

        GameManager.onJoinSession += OnJoin;
        GameManager.onLeaveSession += OnLeave;

        GameManager.onManualClientConnected += AddTarget;
        GameManager.onManualClientDisconnected += RemoveTarget;
    }
    
    void OnDisable(){
        GameManager.onPlayerDeath -= RemoveWeightFromTargetGroup;
        GameManager.onPlayerRevive -= AddWeightFromTargetGroup;

        GameManager.onJoinSession -= OnJoin;
        GameManager.onLeaveSession -= OnLeave;

        GameManager.onManualClientConnected -= AddTarget;
        GameManager.onManualClientDisconnected -= RemoveTarget;
    }

    void LateUpdate(){
        if(playerList.Count > 0 && isActive){
            // Camera Follow
            (Vector3 centerPos, float greatestDistance) = GetCenterPointAndGreatestDistance();
            cameraOrigin.position = Vector3.SmoothDamp(cameraOrigin.position, centerPos, ref camVelocity, smoothTime);

            // Camera zoom to fit players
            float newZoom = Mathf.Lerp(minZoom, maxZoom, (greatestDistance - minZoom) / zoomLimiiter); // t value needs to be between 0 and 1
            cmVirtualCam.m_Lens.FieldOfView = Mathf.Lerp(cmVirtualCam.m_Lens.FieldOfView, newZoom, Time.deltaTime);
        }
    }

    void OnJoin(){
        InitializeTargetGroup();
    }

    void OnLeave(){
        playerList.Clear();
        isActive = false;
    }

    /*
    public void Initialize(Transform _player){
        playerToFollow = _player;
    }
    */

    void InitializeTargetGroup(){
        // playerList.Clear();
        foreach(GameObject playerObject in GameManager.Instance.playerObjects){
            playerList.Add(playerObject.transform);
        }

        isActive = true;
    }

    void AddTarget(ulong playerId){
        GameObject playerObject = GameManager.Instance.GetPlayerObjectByID(playerId);
        playerList.Add(playerObject.transform);
    }

    void RemoveTarget(ulong playerId){
        GameObject playerObject = GameManager.Instance.GetPlayerObjectByID(playerId);
        playerList.Remove(playerObject.transform);
    }

    public void RemoveWeightFromTargetGroup(ulong playerId){
        // Transform playerTransform = GameManager.Instance.playerObjectDict[playerId].transform;
        GameObject playerObject = GameManager.Instance.GetPlayerObjectByID(playerId);
        playerList.Remove(playerObject.transform);
    }

    public void AddWeightFromTargetGroup(ulong playerId){
        // Transform playerTransform = GameManager.Instance.playerObjDict[playerId].transform;
        GameObject playerObject = GameManager.Instance.GetPlayerObjectByID(playerId);
        playerList.Add(playerObject.transform);
    }
    
    (Vector3, float) GetCenterPointAndGreatestDistance(){
        if(playerList.Count <= 0) return (Vector3.zero, minZoom);
        if(playerList.Count == 1){
            return (cameraOrigin.position = playerList[0].position, minZoom);
        }
        
        Bounds bounds = new Bounds(playerList[0].position, Vector3.zero);
        foreach(Transform player in playerList){
            if(player != null){
            bounds.Encapsulate(player.position);

            }
        }

        float newFov = Mathf.Max(bounds.size.x, bounds.size.y);

        return (bounds.center, Mathf.Clamp(minZoom + newFov, minZoom, maxZoom));
    }
}
