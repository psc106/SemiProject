using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //필수 컴퍼넌트
    Rigidbody playerRigidBody;
    Animator animator;
    
    //상수
    public static readonly float WALK_SPEED = 2.5f;
    public static readonly float RUN_SPEED = 4;
    public static readonly float ROTATE_SPEED = 5;
    public static readonly float JUMP_POWER = 3;

    public static readonly float JUMP_LIMIT = 1;
    public static readonly float STOP_MULTIPLE = .8f;

    //상태
    public PlayerState state = PlayerState.IDLE;

    //이동에 필요한 값
    private float rotateAccelX = 0;
    private float yRotate = 0;
    private float xInput = 0;
    private float yInput = 0;

    private int jumpCount = 0;

    private bool isDash = false;
    private bool isJump = false;
    public bool isGround = false;


    //필수 컴포넌트 설정
    void Awake()
    {
        animator = GetComponent<Animator>();
        playerRigidBody = GetComponent<Rigidbody>();
    }

    //입력 없을시 물리연산
    private void FixedUpdate()
    {
        //멈춰있을때 가속도 감소
        AccelStop();
        Move(state);
    }

    //
    void Update()
    {
        //입력한 값들 모두 불러오고 필요한 값 다시 세팅해줌
        GetInputPower();
        SetInputPower();

        //마우스 회전
        MousRotateX();

        SetState();
        //상태에 따라서 애니메이션 변경
        SetAnimator();
    }


    //마우스 y축 기반으로 회전
    public void MousRotateX()
    {
        Quaternion rotatePower = Quaternion.AngleAxis(rotateAccelX + transform.rotation.y, Vector3.up);

        transform.rotation = rotatePower;
    }

    //이동 : 속도
    //이동 속도 직접 지정해서 설정
    public void Move(float _speed)
    {
        //전진방향+사이드방향 벡터 합친후 속도를 곱해서 최종 velocity 결정 
        Vector3 frontV = transform.forward * yInput;
        Vector3 sideV = transform.right * xInput;
        Vector3 moveNormal = (frontV + sideV).normalized;

        playerRigidBody.velocity = moveNormal * _speed + Vector3.up * playerRigidBody.velocity.y;
        
    }


    //이동 : 상태
    //플레이어의 현재 상태에 따라서 상수로 지정한 속도로 이동
    public void Move(PlayerState _state)
    {
        switch (_state)
        {
            case PlayerState.WALK:
                Move(WALK_SPEED);
                break;
            case PlayerState.RUN:
                Move(RUN_SPEED);
                break;
            default:
                break;
        }
    }

    public void Jump()
    {
        if (isGround && isJump && jumpCount < JUMP_LIMIT)
        {
            jumpCount += 1;
            isGround = false;
            float _jumpPower = JUMP_POWER;
            playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, _jumpPower, playerRigidBody.velocity.z);
        }
    }

    public void JumpEnd()
    {
        Debug.LogWarning(isGround);
        Debug.LogWarning(jumpCount);

        if (!isGround)
        {
            isGround = true;
            jumpCount = 0;
        }
    }


    //가속도 감소
    void AccelStop()
    {
        if (!(state == PlayerState.WALK || state == PlayerState.RUN))
        {
            playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x * STOP_MULTIPLE, playerRigidBody.velocity.y, playerRigidBody.velocity.z * STOP_MULTIPLE);
            if (playerRigidBody.velocity.magnitude < .1f)
            {
                playerRigidBody.velocity = Vector3.zero;
                SetState();
            }
        }
    }

    bool IsInputMove()
    {
        return (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
    }

    void GetInputPower()
    {
        rotateAccelX += Input.GetAxis("Mouse X") * ROTATE_SPEED;
        yRotate = Input.GetAxis("Mouse Y");
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");
        isDash = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift);
        isJump = (Input.GetAxisRaw("Jump") != 0 && jumpCount<JUMP_LIMIT); //Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space);
    }

    void SetInputPower()
    {
        animator.SetFloat("VelocityX", xInput);
        animator.SetFloat("VelocityY", yInput);
    }


    void SetState()
    {
        if (!(Input.anyKey || Input.anyKeyDown) && playerRigidBody.velocity == Vector3.zero)
        {
            state = PlayerState.IDLE;
            return;
        }

        if (IsInputMove() )
        {
            if (isDash)
            {
                state = PlayerState.RUN;
                if (isJump && isGround)
                {
                    state = PlayerState.JUMP_RUN;
                }
            }
            else
            {
                state = PlayerState.WALK;
                if (isJump && isGround)
                {
                    state = PlayerState.JUMP_WALK;
                }
            }
        }
        else
        {
            if (isJump && isGround)
            {
                state = PlayerState.JUMP_STOP;
            }
            else
            {
                state = PlayerState.IDLE;
            }

        }

    }


    void SetAnimator()
    {
        switch (state)
        {
            case PlayerState.WALK:
                animator.SetBool("Walk", true);
                animator.SetBool("Run", false);
                break;
            case PlayerState.RUN:
                animator.SetBool("Run", true);
                break;
            case PlayerState.IDLE:
                animator.SetBool("Walk", false);
                animator.SetBool("Run", false);
                break;
            case PlayerState.JUMP_STOP:
            case PlayerState.JUMP_WALK:
            case PlayerState.JUMP_RUN:
                animator.SetTrigger("Jump");
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("in");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrains"))
        {

            Debug.Log("inin");
            JumpEnd();
        }
    }

}

public enum PlayerState
{
    IDLE = 0,
    WALK,
    RUN,
    JUMP_STOP,
    JUMP_WALK,
    JUMP_RUN,
    ATTACK01,
    ATTACK02,
    DEAD
}
