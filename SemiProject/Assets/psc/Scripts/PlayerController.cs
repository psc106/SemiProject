using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //�ʼ� ���۳�Ʈ
    Rigidbody playerRigidBody;
    Animator animator;
    
    //���
    public static readonly float WALK_SPEED = 2.5f;
    public static readonly float RUN_SPEED = 4;
    public static readonly float ROTATE_SPEED = 5;
    public static readonly float JUMP_POWER = 3;

    public static readonly float JUMP_LIMIT = 1;
    public static readonly float STOP_MULTIPLE = .8f;

    //����
    public PlayerState state = PlayerState.IDLE;

    //�̵��� �ʿ��� ��
    private float rotateAccelX = 0;
    private float yRotate = 0;
    private float xInput = 0;
    private float yInput = 0;

    private int jumpCount = 0;

    private bool isDash = false;
    private bool isJump = false;
    public bool isGround = false;


    //�ʼ� ������Ʈ ����
    void Awake()
    {
        animator = GetComponent<Animator>();
        playerRigidBody = GetComponent<Rigidbody>();
    }

    //�Է� ������ ��������
    private void FixedUpdate()
    {
        //���������� ���ӵ� ����
        AccelStop();
        Move(state);
    }

    //
    void Update()
    {
        //�Է��� ���� ��� �ҷ����� �ʿ��� �� �ٽ� ��������
        GetInputPower();
        SetInputPower();

        //���콺 ȸ��
        MousRotateX();

        SetState();
        //���¿� ���� �ִϸ��̼� ����
        SetAnimator();
    }


    //���콺 y�� ������� ȸ��
    public void MousRotateX()
    {
        Quaternion rotatePower = Quaternion.AngleAxis(rotateAccelX + transform.rotation.y, Vector3.up);

        transform.rotation = rotatePower;
    }

    //�̵� : �ӵ�
    //�̵� �ӵ� ���� �����ؼ� ����
    public void Move(float _speed)
    {
        //��������+���̵���� ���� ��ģ�� �ӵ��� ���ؼ� ���� velocity ���� 
        Vector3 frontV = transform.forward * yInput;
        Vector3 sideV = transform.right * xInput;
        Vector3 moveNormal = (frontV + sideV).normalized;

        playerRigidBody.velocity = moveNormal * _speed + Vector3.up * playerRigidBody.velocity.y;
        
    }


    //�̵� : ����
    //�÷��̾��� ���� ���¿� ���� ����� ������ �ӵ��� �̵�
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


    //���ӵ� ����
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
