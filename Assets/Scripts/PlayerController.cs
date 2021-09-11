using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool checkJumpMultiplier = false ;
    private bool canMove = true;
    private bool canFlip = true;

    private int facingDirection = 1;
    private int amountOfJumpLeft;

    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;

    private Rigidbody2D rb;

    private Animator anim;



    public int amountOfjump = 2;

    public float turnTimerSet = 0.2f;
    public float jumpTimerSet = 0.15f;
    public float movementSpeed;
    public float movementSpeedInAir;
    public float wallSlideSpeed;
    public float jumpforce; 
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float airDragMultiplier; //x
    public float airJumpMultiplier; //y
    public float wallHopForce;
    public float wallJumpForce;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
     
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimation();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        CheckSurrounding();
       
    }

    private void CheckIfWallSliding()
    {

        if (isTouchingWall && !isGrounded && rb.velocity.y < 0 && movementInputDirection == facingDirection) 
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckSurrounding()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
     
        isTouchingWall = Physics2D.Raycast(wallCheck.position,transform.right, wallCheckDistance, whatIsGround);
    }

    private  void CheckMovementDirection()
    {
        if(isFacingRight && movementInputDirection <0)
        {
            Flip();
        }else if(!isFacingRight && movementInputDirection >0)
        {
            Flip();
        }

        if(rb.velocity.x < 0.05 && rb.velocity.x > -0.05)
        {
            isWalking = false;
        }
        else
        {
            isWalking = true;
        }
    }

    private void CheckIfCanJump()
    {
       
        if((isGrounded && rb.velocity.y <= 0.01f ))
        {
            amountOfJumpLeft = amountOfjump;
        }

        if(amountOfJumpLeft > 0 )
        {
            canNormalJump = true;
        }
        else
        {
            canNormalJump = false;
        }

        if (isTouchingWall)
        {
            canWallJump = true;
        }
    }

    private void UpdateAnimation()
    {
        anim.SetBool("IsWalking", isWalking);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void  CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        if(Input.GetButtonDown("Jump"))
        {
            if(isGrounded || (amountOfJumpLeft > 0 && isTouchingWall) )
            {
                NormalJump();
            }else
            {
                jumpTimer = jumpTimerSet;

            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchingWall  && !isGrounded  && facingDirection != movementInputDirection )
        {                  
            canMove = false;
            canFlip = false;
            turnTimer = turnTimerSet;
        }

        if(turnTimer >0)
        {
            turnTimer -= Time.deltaTime;
            if(turnTimer <=0)
            {
                canMove = true;
                canFlip = true;
            }
        }
        

        if(checkJumpMultiplier && !Input.GetButton ("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * airJumpMultiplier);
        }
    }

   /* private void Jump()
    {
        
        else if(isWallSliding && movementInputDirection ==0 && canJump) //墙直角上跳
        {
            isWallSliding = false;
            amountOfJumpLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        
        
    }*/

    private void CheckJump()
    {
        if(jumpTimer > 0 )
        {
            if(!isGrounded && isTouchingWall && movementInputDirection !=0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else
            {
                NormalJump();
            }
            jumpTimer -= Time.deltaTime;
        }
        
        
    }

    private void WallJump()
    {
        if ((isWallSliding || isTouchingWall) && movementInputDirection != 0 && canWallJump) //离开墙上
        {
            checkJumpMultiplier = true;
            isWallSliding = false;
            amountOfJumpLeft = amountOfjump;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
        }
    }

    private void NormalJump()
    {
        if (canNormalJump && !isWallSliding)  //地面跳跃
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpforce);
            amountOfJumpLeft--;
            jumpTimer = 0;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            checkJumpMultiplier = true;
        }
    }

    private void  ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && movementInputDirection == 0) //空中掉落
        {
            rb.velocity = new Vector2(airDragMultiplier * rb.velocity.x, rb.velocity.y);
        }
        else   
        {
            if (canMove)
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);

            }
        }

       
        /*else if(!isGrounded && !isWallSliding && movementInputDirection != 0) //空中移动
        {
            Vector2 forceToAdd = new Vector2(movementSpeedInAir * movementInputDirection,0);
            rb.AddForce(forceToAdd);
           
            if (Mathf.Abs(rb.velocity.x) > movementSpeed)
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
            }
        }*/
        
        
        if(isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);

            }
        }
    }

    private void Flip()
    {
        if (!isWallSliding && canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
       
    }

}
