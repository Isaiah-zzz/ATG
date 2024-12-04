using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    // player speed, jumping, and gravity variables
    [SerializeField] private float speed = 12f;
    [SerializeField] private float jumpPower = 20f;
    [SerializeField] private float grav = 3f;
    [SerializeField] private float gravMultiplier = 2.25f;
    [SerializeField] private float maxFallSpeed = 26f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float velPower = 1.1f;
    [SerializeField] float friction = 0.2f;

    // variables for enemy interaction
    private static int maxHealth = 5;
    [SerializeField] private int health = maxHealth;
    public int CurrentHealth => health;
    private bool damageLock = false;
    private const float DEFAULT_SPAWNX = -124f;
    private const float DEFAULT_SPAWNY = -78f;
    private const float HUB_SPAWNX = 34f;
    private const float HUB_SPAWNY = -35f;
    [SerializeField] private float spawnX;
    [SerializeField] private float spawnY;
    [SerializeField] private bool invincible = false;
    [SerializeField] private float invincibleTime = 3f;
    private float invincibleTimer = 0f;
    [SerializeField] private bool debugMode = false;

    // coyote time and jump buffer variables
    [SerializeField] private float coyoteTime = 0.075f;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.075f;
    private float jumpBufferCounter;

    // variables for catapult functionality
    [SerializeField] private float catapultXPower = 4f;
    [SerializeField] private float catapultYPower = 4f;
    [SerializeField] private float catapultXCap = 25f;
    [SerializeField] private float catapultYCap = 25f;
    [SerializeField] private float fightMomentum = 1.03f;
    [SerializeField] private float catapultTimeThresh = 0.6f;
    private bool catapultReady = false;
    private float catapultChargeTime = 0f;
    private bool shiftPressed = false;
    private float xMomentum = 0;
    Vector2 PlayerPosition;

    // Boss object to broadcast jumps to
    GameObject boss;

    // need this stuff
    [SerializeField] private LayerMask groundLayer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    // variables to store references for NPC interaction
    public GameObject npcObj = null;
    public NpcTalk npcScript = null;

    // UI QOL
    public bool canMove;

    // Sound FX clips
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip catapultClip;
    [SerializeField] private AudioClip catapultReadyClip;
    [SerializeField] private AudioClip deathClip;
    private AudioSource footStepsSound;

    Animator animator;

    private void Start()
    {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        //if not in debug mode, set spawn to default spawn
        if (!debugMode)
        {
            spawnX = DEFAULT_SPAWNX;
            spawnY = DEFAULT_SPAWNY;
        }
        else
        {
            spawnX = transform.position.x;
            spawnY = transform.position.y;
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        footStepsSound = GetComponent<AudioSource>();
        boss = GameObject.FindWithTag("InfectedVole");
    }

    private void Update()
    {
        if(!canMove) {
            return;
        }
        

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

        #region Velocity Update

        // update player velocity
        if (catapultChargeTime > 0f && IsGrounded())
        {
            // player cannot move while catapult is charging
            body.velocity = new Vector2(0, 0);

            // flip player to face mouse while aiming catapult
            PlayerPosition = transform.position;
            Vector2 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float xSign = Mathf.Sign(MousePosition.x - PlayerPosition.x);
            if (xSign > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (xSign < -0.01f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            // force for walking acceleration, decleration, etc.
            float targetSpeed = horizontalInput * speed + xMomentum;
            float speedDif = targetSpeed - body.velocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
            body.AddForce(movement * Vector2.right);

            animator.SetFloat("xVelocity", Mathf.Abs(body.velocity.x));
        }

        #endregion

        // allow player to fight momentum from catapult effectively
        // if ((Mathf.Pow(horizontalInput, xMomentum) < 0) && Mathf.Abs(horizontalInput) > .01f)
        if (Mathf.Sign(horizontalInput) != Mathf.Sign(xMomentum) && Mathf.Abs(horizontalInput) > .01f)
        {
            xMomentum /= fightMomentum;
        }

        // friction
        if (IsGrounded() && Mathf.Abs(horizontalInput) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(body.velocity.x), Mathf.Abs(friction));
            amount *= Mathf.Sign(body.velocity.x);
            body.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        //Play walking sound FX
        if (IsGrounded() && (Mathf.Abs(horizontalInput) > 0.01f))
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
        // otherwise decrease coyote time
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            animator.SetBool("isJumping", true);
        }

        animator.SetFloat("yVelocity", body.velocity.y);

        // friction for catapult momentum on x axis
        if (coyoteTimeCounter != 0 && IsGrounded())
        {
            if (Mathf.Abs(xMomentum) < 0.1f)
            {
                xMomentum = 0f;
            }
            xMomentum /= 1.2f;
        }

        // update jump buffer when W or Space pressed, else decrement buffer time
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) { jumpBufferCounter = jumpBufferTime;}
        else { jumpBufferCounter -= Time.deltaTime; }

        // increment catapult charging while shift is held
        if (Input.GetKey(KeyCode.LeftShift) && IsGrounded() && !catapultReady && shiftPressed)
        {
            catapultChargeTime += Time.deltaTime;
        }

        // if catapult has been charged for long enough, set catapultReady to true
        if (catapultChargeTime >= catapultTimeThresh && IsGrounded() && !catapultReady)
        {
            catapultReady = true;
            SoundFXManager.instance.PlaySoundFXClip(catapultReadyClip, transform, 0.5f);
        }

        // check if Lshift is pressed while on ground for catapult
        if (Input.GetKeyDown(KeyCode.LeftShift) && IsGrounded())
        {
            shiftPressed = true;
            animator.SetTrigger("leafJumpReadyTrigger");
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && catapultReady)
            {
                animator.SetTrigger("leafJumpReleaseTrigger");
            }
        }

        // catapult is not ready if shift released
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            shiftPressed = false;
            catapultChargeTime = 0f;
            catapultReady = false;
            animator.Play("Still&Walk");
        }

        // catapult functionality
        if (IsGrounded() && catapultReady && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
        {
            Catapult();
        }

        // normal jump functionality
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && catapultChargeTime == 0f)
        {
            Jump();
        }

        // control variable jump height
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.W))) && body.velocity.y > 0f)
        {
            body.gravityScale = grav *  gravMultiplier;
            // coyoteTimeCounter = 0f;
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

        #region Damage

        // update invincibility
        if (invincibleTimer > 0f)
        {
            invincibleTimer = Mathf.Max(invincibleTimer - Time.deltaTime, 0f);
            if (invincibleTimer == 0f) invincible = false;
        }

        #endregion

        #region Debug

        // set spawnpoint at mouse on right click
        if (debugMode && Input.GetKeyDown(KeyCode.Mouse1))
        {
            spawnX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            spawnY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        }

        // respawn player at spawnpoint
        if (debugMode && Input.GetKeyDown(KeyCode.R))
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

        // set y velocity to 0 to prevent unexpected behaviors
        body.velocity = new Vector2(body.velocity.x, 0);

        // add jump force and reset coyote/jump buffer times
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        body.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);

        boss.BroadcastMessage("Jump");

        //Play Sound FX
        SoundFXManager.instance.PlaySoundFXClip(jumpClip, transform, .5f);
    }

    // long jump functionality
    private void Catapult()
    {
        // store player and mouse positions
        PlayerPosition = transform.position;
        Vector2 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float xSign = Mathf.Sign(MousePosition.x - PlayerPosition.x);
        float ySign = Mathf.Sign(MousePosition.y - PlayerPosition.y);

        // update store appropriate values for launch power
        float xPow = Mathf.Min(catapultXCap, Mathf.Abs(MousePosition.x - PlayerPosition.x) * catapultXPower) * xSign;
        float yPow = Mathf.Min(catapultYCap, Mathf.Abs(MousePosition.y - PlayerPosition.y) * catapultYPower) * ySign;
        xMomentum = xPow;

        // add launch force to player
        body.AddForce(transform.right * xPow, ForceMode2D.Impulse);
        body.AddForce(transform.up * yPow, ForceMode2D.Impulse);

        // reset catapult charging and jump buffer
        shiftPressed = false;
        catapultChargeTime = 0f;
        catapultReady = false;
        jumpBufferCounter = 0f;

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

        // if hub checkpoint, set new spawn location
        if (other.CompareTag("HubCheckpoint"))
        {
            spawnX = HUB_SPAWNX;
            spawnY = HUB_SPAWNY;
        }
    }

    //TODO: Hide any message from NPC and indicator that player can speak
    // clear references when leaving NPC range
    void OnTriggerExit2D(Collider2D other)
    {
        npcObj = null;
        npcScript = null;
    }

    //This just detects enemy collisions
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!damageLock && !invincible)
            {
                damageLock = true;
                DamagePlayer();
            }
        }
    }

    public void Respawn()
    {
        //Reset time scale
        Time.timeScale = 1f;

        //Reset health
        health = 5;

        //Reset position
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

        if (health == 0)
        {
            StartCoroutine(Death());
        }
        else
        {
            invincible = true;
            invincibleTimer = invincibleTime;

            //Play sound FX
            SoundFXManager.instance.PlaySoundFXClip(hurtClip, transform, 1f);

            StartCoroutine(FlashSprite());

            damageLock = false;
        }
    }

    IEnumerator Death()
    {
        //Play death animation
        animator.Play("death");
        //Play death sound fx
        SoundFXManager.instance.PlaySoundFXClip(deathClip, transform, 1f);
        //Wait for animation to finish
        yield return new WaitForSeconds(0.5f);
        //Pause game
        Time.timeScale = 0;
    }

    IEnumerator FlashSprite()
    {
        for (int i = 0; i < (int)(invincibleTime * 2f); i++)
        {
            Color tmp = GetComponent<SpriteRenderer>().color;
            tmp.a = 0f;
            GetComponent<SpriteRenderer>().color = tmp;
            yield return new WaitForSeconds(0.25f);
            tmp = GetComponent<SpriteRenderer>().color;
            tmp.a = 1f;
            GetComponent<SpriteRenderer>().color = tmp;
            yield return new WaitForSeconds(0.25f);
        }
    }

    // checking if player is grounded with raycast
    private bool IsGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

}
