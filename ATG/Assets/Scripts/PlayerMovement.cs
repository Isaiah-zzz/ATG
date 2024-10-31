using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour
{
    // player speed, jump, and gravity variables
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float longJumpPower;
    [SerializeField] private float grav;
    [SerializeField] private float gravMultiplier;
    [SerializeField] private float maxFallSpeed = 26f;
    [SerializeField] private float friction;

    // coyote time and jump buffer variables
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferCounter;
    
    // variables for long jump functionality
    private bool longJumpReady = false;
    private float xMomentum = 0;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    private BoxCollider2D boxCollider;
    private float playerScale;
    private Rigidbody2D body;

    // cameras being used in test scene
    [SerializeField] private GameObject cam1;
    //[SerializeField] private GameObject cam2;

    Animator animator;

    private void Start()
    {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    LineRenderer lr;
    Vector2 DragStartPos;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        lr = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerScale = transform.localScale.x;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        // Vector3 mousePos = Input.mousePosition;

        // flip player direction depending on movement

        // if(horizontalInput > 0.01f){
        //     transform.localScale = new Vector3(playerScale, playerScale, playerScale);
        // } 
        // else if (horizontalInput < -0.01f)
        // {
        //     transform.localScale = new Vector3(-playerScale, playerScale, playerScale);
        // }

        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // very basic detection for changing cameras depending on player position
        // FIXME: needs more robust implementation
        // if(transform.position.x < 13)
        // {
        //     cam1.SetActive(true);
        //     cam2.SetActive(false);
        // }
        // if (transform.position.x > 13)
        // {
        //     cam2.SetActive(true);
        //     cam1.SetActive(false);
        // }

        // cause player to fall faster after being up in air for a bit
        if (body.velocity.y < 0)
        {
            body.gravityScale = grav * gravMultiplier;

            // cap max fall speed
            body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, -maxFallSpeed));
        }

        // when player reaches ground, reset gravity and update coyoteTime
        if(IsGrounded())
        {
            body.gravityScale = grav;
            coyoteTimeCounter = coyoteTime;
            animator.SetBool("isJumping", false);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            animator.SetBool("isJumping", true);
        }

        animator.SetFloat("yVelocity", body.velocity.y);

        // friction for velocity on x axis
        if(coyoteTimeCounter != 0 && IsGrounded())
        {
            if (Mathf.Abs(xMomentum) < 0.1f)
            {
                xMomentum = 0f;
            }
            xMomentum /= 1.1f;
        }

        // update jump buffer
        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // check if Lshift is held
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            longJumpReady = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            longJumpReady = false;
        }

        if (Input.GetMouseButton(0))
        {

        }

        if(IsGrounded() && longJumpReady && jumpBufferCounter > 0f)
        {
            DragStartPos = Camera.main.ScreenToWorldPoint(body.position);
            Vector2 DragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Vector2 _velocity = (DragEndPos - DragStartPos) * longJumpPower;
            float xPow = (DragEndPos.x - DragStartPos.x) * longJumpPower;
            float yPow = Mathf.Min(13, DragEndPos.y - DragStartPos.y) * longJumpPower;
            Vector2 _velocity = new(xPow, yPow);
            print("x: "+ (DragEndPos.x - DragStartPos.x));
            print("y: "+ (DragEndPos.y - DragStartPos.y));
            print("Start: " + (DragStartPos));
            print("End: " + (DragEndPos));
            longJumpReady = false;

            body.velocity = _velocity;
            jumpBufferCounter = 0f;
        }

        // long jump functionality
        // if (IsGrounded() && longJumpReady && jumpBufferCounter > 0f)
        // {
        //     // TODO: tidy this up, 3 input fields into Longjump?
        //     if(Input.GetKey(KeyCode.A))
        //     {
        //         LongJump(0);
        //     }
        //     else if(Input.GetKey(KeyCode.W))
        //     {
        //         LongJump(1);
        //     }
        //     else if(Input.GetKey(KeyCode.D))
        //     {
        //         LongJump(2);
        //     }
        //     jumpBufferCounter = 0f;
        // }
        // // normal jump functionality
        // else if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        // {
        //     Jump();
        //     jumpBufferCounter = 0f;
        // }

        // normal jump functionality
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // variable jump height
        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0f)
        {
            body.gravityScale = grav *  4f;
            coyoteTimeCounter = 0f;
        }

        // update player velocity
        if(longJumpReady && IsGrounded())
        {
            body.velocity = new Vector2(0, body.velocity.y);
        }
        else
        {
            body.velocity = new Vector2(horizontalInput * speed + xMomentum, body.velocity.y);
            animator.SetFloat("xVelocity", Mathf.Abs(body.velocity.x));
        }
        
    }

    // basic jump implementation
    private void Jump(){
        body.velocity = new Vector2(body.velocity.x, jumpPower);
    }

    // TODO: Implement additional control for release direction depending on inputs
    private void LongJump(float num)
    {
        longJumpReady = false;

        if(num == 0)
        {
            xMomentum = longJumpPower * -1f;
            body.velocity = new Vector2(xMomentum, xMomentum * -1f);
        }
        else if(num == 1)
        {
            xMomentum = 0f;
            body.velocity = new Vector2(xMomentum, longJumpPower * 1.5f);
        }
        if (num == 2)
        {
            xMomentum = longJumpPower;
            body.velocity = new Vector2(xMomentum, xMomentum);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name == "Enemy")
        {
            transform.localScale = new Vector3(playerScale, -playerScale, playerScale);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "Enemy")
        {
            transform.localScale = new Vector3(playerScale, -playerScale, playerScale);
        }
    }

    // checking if player is grounded with raycast
    private bool IsGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

}
