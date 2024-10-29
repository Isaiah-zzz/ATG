using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour
{
    // player speed, jump, and gravity variables
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float longJumpPower;
    [SerializeField] private float playerScale;
    [SerializeField] private float grav;
    [SerializeField] private float gravMultiplier;
    [SerializeField] private float maxFallSpeed = 26f;

    // coyote time and jump buffer variables
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferCounter;
    
    // variables for long jump functionality
    private bool longJumpReady = false;
    private float xMomentum = 0;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float dashSpeed;
    [SerializeField] private TrailRenderer tr;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    // cameras being used in test scene
    [SerializeField] private GameObject cam1;
    //[SerializeField] private GameObject cam2;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // flip player direction depending on movement
        if(horizontalInput > 0.01f){
            transform.localScale = new Vector3(playerScale, playerScale, playerScale);
        } 
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-playerScale, playerScale, playerScale);
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
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

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

        // long jump functionality
        if (IsGrounded() && longJumpReady && jumpBufferCounter > 0f)
        {
            // TODO: tidy this up, 3 input fields into Longjump?
            if(Input.GetKey(KeyCode.A))
            {
                LongJump(0);
            }
            else if(Input.GetKey(KeyCode.W))
            {
                LongJump(1);
            }
            else if(Input.GetKey(KeyCode.D))
            {
                LongJump(2);
            }
            jumpBufferCounter = 0f;
        }
        // normal jump functionality
        else if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
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

    // checking if player is grounded with raycast
    private bool IsGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

}
