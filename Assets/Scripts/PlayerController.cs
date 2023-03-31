using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float speed = 1;
    [SerializeField] float jumpForce = 1;

    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;


    public StateMachine<PlayerController> stateMachine;

    Rigidbody2D body;
    Animator animator;

    bool _canMove;
    bool _canJump;
    public bool _canAttack;

    
    bool _isGrounded;
    public bool _isAttacking=false;

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
        stateMachine = new StateMachine<PlayerController>(new Idle(this));
        body = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.StateUpdate();
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
            vertical = -transform.up.y - body.gravityScale;
        }



        body.velocity = new Vector2(horizontal, vertical);
    }


    class Idle : IState<PlayerController>
    {
        public Idle(PlayerController owner)
        {
            this.owner = owner;
        }

        public override void Enter()
        {
            owner._canMove = true;
            owner._canJump = true;
            owner._canAttack = true;
        }

        public override void Execute()
        {
            owner.HandleMovements();
            owner.HandleAttack();

            if (owner.body.velocity.x > 0.3)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new Moving(owner));
            }

            if (!owner._isGrounded)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new OnAir(owner));
            }

            if (owner._isAttacking)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new Attacking(owner));
            }

        }

    }

    class Moving : IState<PlayerController>
    {
        
        public Moving(PlayerController owner)
        {
            this.owner = owner;
        }

        public override void Enter()
        {
            owner._canJump = true;
            owner._canAttack = false;
            owner._canMove = true;
        }

        public override void Execute()
        {
            owner.HandleMovements();
            owner.HandleAttack();

            if (owner.body.velocity.x < 0.3)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new Idle(owner));
            }

            if (!owner._isGrounded)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new OnAir(owner));
            }

            

        }

    }

    class OnAir : IState<PlayerController>
    {
        public OnAir(PlayerController owner)
        {
            this.owner = owner;
        }

        public override void Enter()
        {
            owner._canJump = false;
            owner._canAttack = false;
            owner._canMove = false;
        }

        public override void Execute()
        {
            owner.HandleMovements();

            if (owner._isGrounded)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new Idle(owner));
            }
        }

    }

    class Attacking : IState<PlayerController>
    {
        public Attacking(PlayerController owner)
        {
            this.owner = owner;
        }

        public override void Enter()
        {
            owner._canJump = false;
            owner._canAttack = true;
            owner._canMove = false;

            owner.Attack();
        }

        public override void Execute()
        {
            owner.HandleMovements();

            if (!owner._isAttacking)
            {
                owner.stateMachine = new StateMachine<PlayerController>(new Idle(owner));
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
