using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    #region private Field

    PlayerController playerController;
    private float pushPower = 10;

    #endregion

    #region public Field

    public GameObject weapon;
    public float health = 1f;
    public float hitMultiplier = 0;

    #endregion

    #region MonoBehaviour Callback

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        

        if (photonView.IsMine)
        {
            CinemachineVirtualCamera camera = GameObject.Find("FollowCam").GetComponent<CinemachineVirtualCamera>();
            camera.Follow = transform;
            camera.LookAt = transform.Find("HeadPosition");

        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
/*
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }*/
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            //처리?
            if(health <= 0f)
            {
                _GameManager.instance.LeaveRoom();
            }
        }
    }

    [PunRPC]
    private void OnHit(Vector3 contact)
    {
        hitMultiplier += Time.fixedDeltaTime* pushPower;
        playerController.Push(hitMultiplier, contact);
        OnHitReaction();
    }

    private void OnHitReaction()
    {
        playerController.Stun();
    }

    // 애니메이션 트리거를 설정하는 함수
    public void TriggerAnimation(string triggerName)
    {
        photonView.RPC("SetAnimationTrigger", RpcTarget.All, triggerName);
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.transform == transform)
        {
            return;
        }
        if (!other.name.Equals("Attack"))
        {
            return;
        }



        if (other.GetComponent<WeaponController>().rootObject.IsAttacking() && photonView.IsMine)
        {
            photonView.RPC("OnHit", RpcTarget.All, other.GetComponent<WeaponController>().rootObject.transform.position.normalized);
            coroutine = StartCoroutine(StunDelayRoutine(3f));
            //OnHit(other.ClosestPointOnBounds(transform.position));
        }
       /* if (PhotonNetwork.IsMasterClient)
        {

        }*/

    }

    private void OnTriggerStay(Collider other)
    {

        if (other.transform == transform)
        {
            return;
        }
        if (!other.name.Equals("Attack"))
        {
            return;
        }



        if (other.GetComponent<WeaponController>().rootObject.IsAttacking() && photonView.IsMine)
        {
            if (!other.GetComponent<WeaponController>().rootObject.isStun && playerController.isInvincible)
            {
                photonView.RPC("OnHit", RpcTarget.All, other.GetComponent<WeaponController>().rootObject.transform.position.normalized);
                coroutine = StartCoroutine(StunDelayRoutine(3f));
            }
            //OnHit(other.ClosestPointOnBounds(transform.position));
        }
        /*if (PhotonNetwork.IsMasterClient)
        {
            if (other.GetComponent<PlayerManager>().playerController.IsAttacking())
            {
                OnHit(other.ClosestPointOnBounds(transform.position));
            }

        }*/
    }

    private void OnTriggerExit(Collider other)
    {
       // hitMultiplier = 0;
    }

    Coroutine coroutine = null;
    public IEnumerator StunDelayRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        playerController.StunEnd();
        hitMultiplier = 0;
        coroutine = null;
    }

    #endregion
}
