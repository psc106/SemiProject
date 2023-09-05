using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Recorder.OutputPath;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    //�ʼ� ���۳�Ʈ
    Rigidbody playerRigidBody;
    Animator animator;
    Transform root;
    
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

    private bool canAttack = true;
    [SerializeField]
    private bool canRall = true;

    private bool isPush = false;
    [SerializeField]
    private bool isRall = false;
    private bool isDash = false;
    private bool isJump = false;
    private bool isAttack = false;

    public bool isInvincible = false;
    public bool isStun = false;
    public bool isGround = false;
    public bool attacking = false;


    //�ʼ� ������Ʈ ����
    void Awake()
    {
        animator = GetComponent<Animator>();
        playerRigidBody = GetComponent<Rigidbody>();
        gameObject.name = "Player"+gameObject.GetPhotonView().ViewID ;
        root = transform.Find("RotateRoot");
        Cursor.lockState = CursorLockMode.Confined;
    }

    //�Է� ������ ��������
    private void FixedUpdate()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        //���������� ���ӵ� ����
        //AccelStop();
        if (IsInputMove() && !isStun && canRall)
        {
            Move(state);
        }
    }

    //
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (!isStun)
        {
            //�Է��� ���� ��� �ҷ����� �ʿ��� �� �ٽ� ��������
            GetInputPower();
            SetInputPower();

            //���콺 ȸ��
            MousRotateX();
        }

        SetState();
        //���¿� ���� �ִϸ��̼� ����
        SetAnimator();

    }


    //���콺 y�� ������� ȸ��
    public void MousRotateX()
    {
        /*
                Quaternion rotatePower = Quaternion.AngleAxis(rotateAccelX + transform.rotation.y, Vector3.up);

                transform.rotation = rotatePower;*/

        // ���콺 X �Է¿� ����Ͽ� ȸ������ ����մϴ�.
        float mouseX = Input.GetAxis("Mouse X") * ROTATE_SPEED;

        // ���� ������Ʈ�� ȸ������ ���콺 �Է¿� ���� ȸ������ ���մϴ�.
        Quaternion rotationDelta = Quaternion.Euler(0, mouseX, 0);
        Quaternion targetRotation = transform.rotation * rotationDelta;

        // ȸ������ �����մϴ�.
        transform.rotation = targetRotation;
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
        Quaternion targetRotation = Quaternion.LookRotation(moveNormal);
        root.localRotation = targetRotation;


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


    public void Stun()
    {
        isStun = true;

    }
    public void StunEnd()
    {
        isStun = false;
        isPush = false;

    }

    public void Attack()
    {
        attacking = true;

    }

    public void AttackEnd()
    {
        attacking = false;
    }

    public void Push(float power, Vector3 contact)
    {
        if (!isPush)
        {
            isPush = false;
            playerRigidBody.velocity = Vector3.zero;
        }
        Vector3 resultPower = (transform.position - contact);
        playerRigidBody.AddForce(new Vector3(resultPower.x, 0, resultPower.z) * power * 100, ForceMode.VelocityChange);
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

        if (!isGround)
        {
            isGround = true;
            jumpCount = 0;
        }
    }

    public void Rall()
    {
        Vector3 forword = transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 side = transform.right * Input.GetAxisRaw("Horizontal");
        transform.LookAt((forword + side).normalized);
        playerRigidBody.AddForce((forword + side).normalized*7f, ForceMode.VelocityChange);
        canRall = false;
        isInvincible = true;
    }

    public void InvincibleEnd()
    {
        isInvincible = false;
    }

    public void RallEnd()
    {
        canRall = true;
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

    public bool IsAttacking()
    {
        return attacking;

    }

    bool CanJump()
    {
        return (isJump && isGround && canAttack);
    }

    void GetInputPower()
    {
        rotateAccelX += Input.GetAxis("Mouse X") * ROTATE_SPEED;
        yRotate = Input.GetAxis("Mouse Y");
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");
        isDash = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift);
        isJump = (Input.GetAxisRaw("Jump") != 0 && jumpCount < JUMP_LIMIT); //Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space);
        isAttack = (Input.GetAxisRaw("MeeleAttack") != 0 && canAttack); //Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space);
        isRall = (Input.GetAxisRaw("Rall") != 0 && canRall);
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
                if (CanJump())
                {
                    state = PlayerState.JUMP_RUN;
                }
            }
            else
            {
                state = PlayerState.WALK;
                if (CanJump())
                {
                    state = PlayerState.JUMP_WALK;
                }
            }
        }
        else
        {
            if (CanJump())
            {
                state = PlayerState.JUMP_STOP;
            }
            else
            {
                state = PlayerState.IDLE;
            }
        }

        if (isRall && canRall && canAttack)
        {
            state = PlayerState.RALL;

        } 
        else
        if (isAttack && canAttack && canRall)
        {
            state = PlayerState.MEELE;
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
                TriggerAnimation("Jump");
                break;
            case PlayerState.MEELE:
                TriggerAnimation("MeeleAttack");
                break;
            case PlayerState.RALL:
                TriggerAnimation("Rall");
                break;
        }
    }

    [PunRPC]
    private void SetAnimationTrigger(string triggerName)
    {
        // Ʈ���� ����
        animator.SetTrigger(triggerName);
    }

    // �ִϸ��̼� Ʈ���Ÿ� �����ϴ� �Լ�
    public void TriggerAnimation(string triggerName)
    {
        photonView.RPC("SetAnimationTrigger", RpcTarget.All, triggerName);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<PlayerManager>().health);
        }
        else
        {
            GetComponent<PlayerManager>().health = (float)stream.ReceiveNext();
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
    MEELE,
    ATTACK02,
    RALL,
    DEAD
}
