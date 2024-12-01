using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour
{
    // player speed, jumping, and gravity variables
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float grav;
    [SerializeField] private float gravMultiplier;
    [SerializeField] private float maxFallSpeed = 26f;
    [SerializeField] float acceleration = 3f;
    [SerializeField] float deceleration = 5f;
    [SerializeField] float velPower = 1.1f;
    [SerializeField] float friction = 0.2f;

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

    // variables for catapult functionality
    [SerializeField] private float catapultXPower;
    [SerializeField] private float catapultYPower;
    [SerializeField] private float catapultXCap;
    [SerializeField] private float catapultYCap;
    [SerializeField] private float fightMomentum = 1.2f;
    private bool catapultReady = false;
    private float catapultCharging = 0f;
    [SerializeField] private float catapultTimeThresh = 0.6f;
    private float xMomentum = 0;
    Vector2 PlayerPosition;

    // need this stuff
    [SerializeField] private LayerMask groundLayer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    // cameras being used in test scene
    [SerializeField] private GameObject cam1;

    // variables to store references for NPC interaction
    public GameObject npcObj = null;
    public NpcTalk npcScript = null;

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

        #region Walking

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

        // update player velocity
        if (catapultCharging > 0f && IsGrounded())
        {
            body.velocity = new Vector2(0, 0);
        }
        else
        {
            float targetSpeed = horizontalInput * speed + xMomentum;
            float speedDif = targetSpeed - body.velocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
            body.AddForce(movement * Vector2.right);

            animator.SetFloat("xVelocity", Mathf.Abs(body.velocity.x));
        }

        // allow player to fight momentum
        if ((Mathf.Pow(horizontalInput, xMomentum) < 0) && Mathf.Abs(horizontalInput) > .01f)
        {
            xMomentum /= fightMomentum;
        }

        if (IsGrounded() && Mathf.Abs(horizontalInput) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(body.velocity.x), Mathf.Abs(friction));
            amount *= Mathf.Sign(body.velocity.x);
            body.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
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

        #endregion

        #region Jumping

        // cause player to fall faster after being up in air for a bit
        if (body.velocity.y < 0 && coyoteTimeCounter <= 0)
        {
            body.gravityScale = grav * gravMultiplier;
        }

        // cap max fall speed
        if (body.velocity.y < -maxFallSpeed)
        {
            body.velocity = new Vector2(body.velocity.x, -maxFallSpeed);
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

        animator.SetFloat("yVelocity", body.velocity.y);

        // friction for momentum on x axis
        if (coyoteTimeCounter != 0 && IsGrounded())
        {
            if (Mathf.Abs(xMomentum) < 0.1f)
            {
                xMomentum = 0f;
            }
            xMomentum /= fightMomentum;
        }

        // update jump buffer
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) { jumpBufferCounter = jumpBufferTime;}
        else { jumpBufferCounter -= Time.deltaTime; }

        if (catapultCharging >= catapultTimeThresh && IsGrounded())
        {
            catapultReady = true;
        }

        if (Input.GetKey(KeyCode.LeftShift) && IsGrounded())
        {
            catapultCharging += Time.deltaTime;
        }

        // check if Lshift is held
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetTrigger("leafJumpReadyTrigger");
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && catapultReady)
            {
                animator.SetTrigger("leafJumpReleaseTrigger");
            }
        }

        // catapult is not ready if shift released
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            catapultCharging = 0f;
            catapultReady = false;
            animator.Play("Still&Walk");
        }

        // FIXME: make catapult work 100% of the time
        // catapult functionality
        if (IsGrounded() && catapultReady && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
        {
            catapultReady = false;
            catapultCharging = 0f;
            catapult();
            jumpBufferCounter = 0f;
        }

        // normal jump functionality
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && catapultCharging == 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // control variable jump height
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.W))) && body.velocity.y > 0f)
        {
            body.gravityScale = grav *  gravMultiplier;
            coyoteTimeCounter = 0f;
        }

        #endregion

        #region NPC Interaction

        //TODO: show some kind of indication when the player can talk
        // activate NPC dialog if available
        if (Input.GetKeyDown(KeyCode.T) && npcObj)
        {
            npcScript.Talk();
        }

        #endregion

        #region Debug

        // set spawnpoint
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            spawnX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            spawnY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        }

        // "respawn" player
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }

        // talking animation
        if (Input.GetKeyDown(KeyCode.T))
        {
            animator.SetTrigger("talkTrigger");
        }

        // respawn player if falling into void
        if (transform.position.y < -120)
        {
            Respawn();
        }

        #endregion
    }

    // basic jump implementation
    private void Jump(){

        // body.velocity = new Vector2(body.velocity.x, jumpPower);
        body.velocity = new Vector2(body.velocity.x, 0);
        body.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;

        // short hop if jump buffer used and jump key not held
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.W))
        {
            body.gravityScale = grav * gravMultiplier;
        }

            //Play Sound FX
            SoundFXManager.instance.PlaySoundFXClip(jumpClip, transform, .5f);
    }

    // TODO: Attach animations so player orientation faces the right way on launch
    // make the player turn to face the arrow while charging the catapult

    // long jump functionality
    private void catapult()
    {
        // store player and mouse positions
        PlayerPosition = transform.position;
        Vector2 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float xSign = Mathf.Sign(MousePosition.x - PlayerPosition.x);
        float ySign = Mathf.Sign(MousePosition.y - PlayerPosition.y);

        // update player velocity
        float xPow = Mathf.Min(catapultXCap, Mathf.Abs(MousePosition.x - PlayerPosition.x) * catapultXPower) * xSign;
        float yPow = Mathf.Min(catapultYCap, Mathf.Abs(MousePosition.y - PlayerPosition.y) * catapultYPower) * ySign;
        xMomentum = xPow;

        body.AddForce(transform.right * xPow, ForceMode2D.Impulse);
        body.AddForce(transform.up * yPow, ForceMode2D.Impulse);

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

        //Despawn all cornstalk and popcorn objects
        foreach (var cornstalk in GameObject.FindGameObjectsWithTag("Cornstalk"))
        {
            Destroy(cornstalk);
        }
        foreach (var popcorn in GameObject.FindGameObjectsWithTag("Popcorn"))
        {
            Destroy(popcorn);
        }
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
