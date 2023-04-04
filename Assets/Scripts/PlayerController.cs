using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStateType
{
    Idle,
    Walk,
    OnAir,
    Attack
}


public class PlayerController : MonoBehaviour
{

    [SerializeField] float speed = 1;
    [SerializeField] float jumpForce = 1;

    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;


    public StateMachine<PlayerStateType> StateMachine { get; } = new();

    Rigidbody2D body;
    Animator animator;

    bool _canMove;
    bool _canJump;
    public bool _canAttack;

    
    bool _isGrounded;
    public bool _isAttacking=false;
     bool _isJumping;

    public bool readyToJump
    {
        get
        {
            if (_isGrounded  && _canJump)
                return true;
            else
                return false;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        StateMachine.RegisterState(PlayerStateType.Idle, new PlayerIdleState(this));
        StateMachine.RegisterState(PlayerStateType.Walk, new PlayerWalkState(this));
        StateMachine.RegisterState(PlayerStateType.OnAir, new PlayerOnAirState(this));
        StateMachine.RegisterState(PlayerStateType.Attack, new PlayerAttackingState(this));

        StateMachine.SetState(PlayerStateType.Idle);

        body = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.StateUpdate();
    }

    private void FixedUpdate()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundMask);
    }

    private void HandleAttack()
    {
        if(_canAttack && !_isAttacking && Input.GetMouseButtonDown(0))
        {
            _isAttacking = true;
        }
    }

    float horizontal;
    private void HandleMovements()
    {
        if (_isGrounded)
        {
            horizontal = 0;
            animator.SetBool("onAir", false);
        }
        else
            animator.SetBool("onAir", true);

        if (_canMove)
        {
            horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * speed;

            if (horizontal > 0)
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            else if (horizontal < 0)           
                gameObject.transform.localScale = new Vector3(-1, 1, 1);

            animator.SetFloat("speed", Mathf.Abs(horizontal));
               
        }

        float vertical = 0;

        if (readyToJump && Input.GetKeyDown(KeyCode.Space))
        {
            vertical = transform.up.y * jumpForce;
        }
        else
        {
            vertical = Vector2.down.y - 3;
        }



        body.velocity = new Vector2(horizontal, vertical);
    }


    class PlayerIdleState : State
    {
        PlayerController playerController;
        public PlayerIdleState(PlayerController owner)
        {
            playerController = owner;
        }

        public override void Enter()
        {
            playerController._canMove = true;
            playerController._canJump = true;
            playerController._canAttack = true;
        }

        public override void Execute()
        {
            playerController.HandleMovements();
            playerController.HandleAttack();

            if (Mathf.Abs(playerController.body.velocity.x) > 0.05)
            {
                playerController.StateMachine.SetState(PlayerStateType.Walk);
            }

            if (!playerController._isGrounded)
            {
                playerController.StateMachine.SetState(PlayerStateType.OnAir);
            }

            if (playerController._isAttacking)
            {
                playerController.StateMachine.SetState(PlayerStateType.Attack);
            }

        }

    }

    class PlayerWalkState : State
    {
        PlayerController playerController;

        public PlayerWalkState(PlayerController owner)
        {
            playerController = owner;
        }

        public override void Enter()
        {
            playerController._canJump = true;
            playerController._canAttack = true;
            playerController._canMove = true;
        }

        public override void Execute()
        {
            playerController.HandleMovements();
            playerController.HandleAttack();

            if (Mathf.Abs(playerController.body.velocity.x) < 0.05)
            {
                playerController.StateMachine.SetState(PlayerStateType.Idle);
            }

            if (!playerController._isGrounded)
            {
                playerController.StateMachine.SetState(PlayerStateType.OnAir);
            }

            if (playerController._isAttacking)
            {
                playerController.StateMachine.SetState(PlayerStateType.Attack);
            }

        }

    }

    class PlayerOnAirState : State
    {
        PlayerController playerController;
        public PlayerOnAirState(PlayerController owner)
        {
            playerController = owner;
        }

        public override void Enter()
        {
            playerController._canJump = false;
            playerController._canAttack = false;
            playerController._canMove = false;
        }

        public override void Execute()
        {
            playerController.HandleMovements();

            if (playerController._isGrounded)
            {
                playerController.StateMachine.SetState(PlayerStateType.Idle);
            }
        }

    }



    class PlayerAttackingState : State
    {
        PlayerController playerController;
        public PlayerAttackingState(PlayerController owner)
        {
            playerController = owner;
        }

        public override void Enter()
        {
            playerController._canJump = false;
            playerController._canAttack = true;
            playerController._canMove = false;

            playerController.Attack();
        }

        public override void Execute()
        {
            playerController.HandleMovements();

            if (!playerController._isAttacking)
            {
                playerController.StateMachine.SetState(PlayerStateType.Idle);
            }
        }

    }

    private void Attack()
    {
        animator.SetTrigger("attack");
        Debug.Log("attack");
    }

    public void AttackEndFromAnimation()
    {
        _isAttacking = false;
    }
}
