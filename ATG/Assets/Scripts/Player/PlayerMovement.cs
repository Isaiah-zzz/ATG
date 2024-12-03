using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    // player speed, jumping, and gravity variables
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float grav;
    [SerializeField] private float gravMultiplier;
    [SerializeField] private float maxFallSpeed = 26f;

    // variables for enemy interaction
    private static int maxHealth = 5;
    [SerializeField] private int health = maxHealth;
    public int CurrentHealth => health;
    private bool damageLock = false;
    [SerializeField] private float spawnX;
    [SerializeField] private float spawnY;
    [SerializeField] private bool invincible = false;

    // coyote time and jump buffer variables
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferCounter;

    // variables for long jump functionality
    [SerializeField] private float catapultXPower;
    [SerializeField] private float catapultYPower;
    [SerializeField] private float catapultXCap;
    [SerializeField] private float catapultYCap;
    private bool catapultReady = false;
    private float xMomentum = 0;
    Vector2 PlayerPosition;

    // need this stuff
    [SerializeField] private LayerMask groundLayer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    // cameras being used in test scene
    [SerializeField] private GameObject cam1;
    //[SerializeField] private GameObject cam2

    // variables to store references for NPC interaction
    public GameObject npcObj = null;
    public NpcTalk npcScript = null;

    // UI QOL
    public bool canMove;

    // Sound FX clips
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip catapultClip;
    private AudioSource footStepsSound;

    Animator animator;

    private void Start()
    {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        spawnX = transform.position.x;
        spawnY = transform.position.y;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        footStepsSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(!canMove) {
            return;
        }
        
        // get and store horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        // flip player direction depending on movement
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // cause player to fall faster after being up in air for a bit
        if (body.velocity.y < 0)
        {
            body.gravityScale = grav * gravMultiplier;

            // cap max fall speed
            body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, -maxFallSpeed));
        }

        // when player reaches ground, reset gravity and update coyoteTime
        if (IsGrounded())
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

        // allow player to actually fight momentum while in midair
        if (!IsGrounded() && (Mathf.Pow(horizontalInput, xMomentum) < 0) && Mathf.Abs(horizontalInput) > .01f)
        {
            xMomentum /= 1.1f;
        }

        animator.SetFloat("yVelocity", body.velocity.y);

        // friction for velocity on x axis
        if (coyoteTimeCounter != 0 && IsGrounded())
        {
            if (Mathf.Abs(xMomentum) < 0.1f)
            {
                xMomentum = 0f;
            }
            xMomentum /= 1.1f;
        }

        // update jump buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // NOTE: For testing only
        // set spawnpoint
        if (Input.GetKeyDown(KeyCode.H)){
            spawnX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            spawnY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        }

        // NOTE: For testing only
        // "respawn" player
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }

        // NOTE: For testing only
        // talking animation
        if (Input.GetKeyDown(KeyCode.T))
        {
            animator.SetTrigger("talkTrigger");
        }

        // NOTE: For testing only
        // respawn player if falling into void
        if (transform.position.y < -120)
        {
            Respawn();
        }


        // check if Lshift is held
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            catapultReady = true;
            animator.SetTrigger("leafJumpReadyTrigger");
            if (Input.GetKeyDown(KeyCode.Space)){
                animator.SetTrigger("leafJumpReleaseTrigger");
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            catapultReady = false;
            animator.Play("Still&Walk");
        }

        // catapult functionality
        if (IsGrounded() && catapultReady && jumpBufferCounter > 0f)
        {
            catapult();

            catapultReady = false;
            jumpBufferCounter = 0f;
        }

        // normal jump functionality
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // control variable jump height
        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0f)
        {
            body.gravityScale = grav *  4f;
            coyoteTimeCounter = 0f;
        }

        // update player velocity
        if (catapultReady && IsGrounded())
        {
            body.velocity = new Vector2(0, body.velocity.y);
        }
        else
        {
            body.velocity = new Vector2(horizontalInput * speed + xMomentum, body.velocity.y);
            animator.SetFloat("xVelocity", Mathf.Abs(body.velocity.x));
        }

        //TODO: show some kind of indication when the player can talk
        // activate NPC dialog if available
        if (Input.GetKeyDown(KeyCode.T) && npcObj)
        {
            npcScript.Talk();
        }

        //Play walking sound FX
        if (IsGrounded() && (horizontalInput > 0.01f || horizontalInput < -0.01f))
        {
            footStepsSound.enabled = true;
        }
        else
        {
            footStepsSound.enabled = false;
        }
    }

    // basic jump implementation
    private void Jump(){
        body.velocity = new Vector2(body.velocity.x, jumpPower);

        //Play Sound FX
        SoundFXManager.instance.PlaySoundFXClip(jumpClip, transform, .5f);
    }

    // TODO: Attach animations so player orientation faces the right way on launch
    // long jump functionality
    private void catapult()
    {
        // store player and mouse positions
        PlayerPosition = transform.position;
        Vector2 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float xSign = 1;
        float ySign = 1;

        // determine pos/neg sign for x & y directions
        if (MousePosition.x < PlayerPosition.x)
        {
            xSign = -1;
        }
        if (MousePosition.y < PlayerPosition.y)
        {
            ySign = -1;
        }

        // update player velocity
        float xPow = Mathf.Min(catapultXCap, Mathf.Abs(MousePosition.x - PlayerPosition.x) * catapultXPower) * xSign;
        float yPow = Mathf.Min(catapultYCap, Mathf.Abs(MousePosition.y - PlayerPosition.y) * catapultYPower) * ySign;
        xMomentum = xPow;
        Vector2 _velocity = new(xPow, yPow);
        body.velocity = _velocity;

        //Play Sound FX
        SoundFXManager.instance.PlaySoundFXClip(catapultClip, transform, .75f);
    }

    // detect when the player has entered the range of an npc
    void OnTriggerEnter2D (Collider2D other)
    {
        // if player is within range of NPC
        if (other.CompareTag("NPC"))
        {
            npcObj = other.gameObject;
            npcScript = npcObj.GetComponent<NpcTalk>();
        }

        // if player makes contact with enemy
        if (other.CompareTag("Enemy"))
        {
            if (!damageLock && !invincible)
            {
                damageLock = true;
                DamagePlayer();
            }
        }
    }

    //TODO: Hide any message from NPC and indicator that player can speak
    // clear references when leaving NPC range
    void OnTriggerExit2D(Collider2D other)
    {
        npcObj = null;
        npcScript = null;
    }

    void Respawn()
    {
        transform.position = new Vector2(spawnX, spawnY);
        body.velocity = new Vector2(0, 0);
        xMomentum = 0;
    }

    void DamagePlayer()
    {
        health--;

        //Play sound FX
        SoundFXManager.instance.PlaySoundFXClip(hurtClip, transform, 1f);

        if(health != 0)
        {
            Respawn();
        }
        else
        {
            // TODO: respawn player on game over
            // Game over, respawn at last "save point"
            // maybe make the dude explode into wheat or something along those
            // lines that isn't too hard to animate
            print("Game over!");
        }

        damageLock = false;
    }

    // checking if player is grounded with raycast
    private bool IsGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

}
